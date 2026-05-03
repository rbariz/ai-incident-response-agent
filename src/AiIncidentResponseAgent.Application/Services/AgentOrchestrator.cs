using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Application.Models;
using AiIncidentResponseAgent.Domain.Actions;
using AiIncidentResponseAgent.Domain.Events;
using AiIncidentResponseAgent.Domain.Executions;
using AiIncidentResponseAgent.Domain.Incidents;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiIncidentResponseAgent.Application.Services;

public sealed class AgentOrchestrator : IAgentOrchestrator
{
    private readonly IAgentEventRepository _events;
    private readonly IAgentExecutionRepository _executions;
    private readonly IIncidentRepository _incidents;
    private readonly IAgentAnalyzer _analyzer;
    private readonly IAgentDecisionEngine _decisionEngine;
    private readonly IAgentPolicyEngine _policyEngine;
    private readonly IAgentActionExecutor _actionExecutor;
    private readonly IAgentFeedbackHandler _feedbackHandler;
    private readonly IAgentMemoryService _memoryService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAgentActionLockRepository _actionLocks;
    private readonly ITextTranslator _translator;
    private readonly IRealtimeNotifier _realtime;
    private readonly RetryOptions _retryOptions;
    private readonly ILogger<AgentOrchestrator> _logger;
    private readonly IAuditService _audit;

    public AgentOrchestrator(
        IAgentEventRepository events,
        IAgentExecutionRepository executions,
        IIncidentRepository incidents,
        IAgentAnalyzer analyzer,
        IAgentDecisionEngine decisionEngine,
        IAgentPolicyEngine policyEngine,
        IAgentActionExecutor actionExecutor,
        IAgentFeedbackHandler feedbackHandler,
        IAgentMemoryService memoryService,
        IUnitOfWork unitOfWork,
        IAgentActionLockRepository actionLocks,
        ITextTranslator translator,
        IRealtimeNotifier realtime,
        IOptions<RetryOptions> retryOptions,
        ILogger<AgentOrchestrator> logger,
        IAuditService audit)
    {
        _events = events;
        _executions = executions;
        _incidents = incidents;
        _analyzer = analyzer;
        _retryOptions = retryOptions.Value;
        _decisionEngine = decisionEngine;
        _policyEngine = policyEngine;
        _actionExecutor = actionExecutor;
        _feedbackHandler = feedbackHandler;
        _memoryService = memoryService;
        _unitOfWork = unitOfWork;
        _actionLocks = actionLocks;
        _translator = translator;
        _realtime = realtime;
        _logger = logger;
        _audit = audit;
    }

    public async Task ProcessEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var agentEvent = await _events.GetByIdAsync(eventId, cancellationToken);

        if (agentEvent is null)
        {
            return;
        }
        using var logScope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["EventId"] = agentEvent.Id,
            ["CorrelationId"] = agentEvent.CorrelationId,
            ["EventType"] = agentEvent.Type.ToString()
        });

        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Agent event processing started. EventType={EventType}, CorrelationId={CorrelationId}",
            agentEvent.Type,
            agentEvent.CorrelationId);

        var idempotencyKey = $"action-event:{agentEvent.Id}";

        var existingExecution = await _executions.GetByIdempotencyKeyAsync(
            idempotencyKey,
            cancellationToken);

        if (existingExecution is not null)
        {
            _logger.LogInformation(
    "Agent event skipped because execution already exists. IdempotencyKey={IdempotencyKey}",
    idempotencyKey);
            return;
        }

        var execution = new AgentExecution(
            agentEvent.Id,
            idempotencyKey,
            agentEvent.CorrelationId);

        await _executions.AddAsync(execution, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        execution.Start();

        _logger.LogInformation(
    "Agent execution started. ExecutionId={ExecutionId}",
    execution.Id);

        await _realtime.AgentExecutionStartedAsync(
            execution.Id,
            agentEvent.Id,
            agentEvent.CorrelationId,
            cancellationToken);

        try
        {
            var initialContext = new AgentContext
            {
                Event = agentEvent,
                Lang = agentEvent.Lang
            };

            var memoryJson = await _memoryService.LoadMemoryAsync(
                initialContext,
                cancellationToken);

            var context = new AgentContext
            {
                Event = agentEvent,
                MemoryJson = memoryJson,
                Lang = agentEvent.Lang
            };

            var analysis = await _analyzer.AnalyzeAsync(context, cancellationToken);

            var analysisLang = context.Lang;

            var analysisSummaryFr = analysisLang == "fr"
                ? analysis.Summary
                : await _translator.TranslateAsync(
                    analysis.Summary,
                    analysisLang,
                    "fr",
                    cancellationToken);

            var analysisSummaryEn = analysisLang == "en"
                ? analysis.Summary
                : await _translator.TranslateAsync(
                    analysis.Summary,
                    analysisLang,
                    "en",
                    cancellationToken);

            var decision = await _decisionEngine.DecideAsync(
                context,
                analysis,
                cancellationToken);

            _logger.LogInformation(
    "AI analysis completed. ExecutionId={ExecutionId}, Provider={Provider}, Language={Language}, Confidence={ConfidenceScore}",
    execution.Id,
    analysis.Provider,
    analysisLang,
    analysis.ConfidenceScore);

            execution.SetDecision(
                decision.Decision,
                decision.Action,
                analysis.Summary,
                analysis.ConfidenceScore,
                analysis.Provider,
                analysisLang,
                analysisSummaryFr,
                analysisSummaryEn);

            _logger.LogInformation(
    "Decision computed. ExecutionId={ExecutionId}, Decision={Decision}, Action={Action}, RequiresHumanApproval={RequiresHumanApproval}",
    execution.Id,
    decision.Decision,
    decision.Action,
    decision.RequiresHumanApproval);

            Incident? incident = null;

            if (decision.Severity >= IncidentSeverity.Medium)
            {
                incident = new Incident(
                    agentEvent.Id,
                    title: $"Agent incident - {agentEvent.Type}",
                    description: decision.Reason,
                    severity: decision.Severity);

                incident.MarkAnalyzing();

                await _incidents.AddAsync(incident, cancellationToken);
                execution.AttachIncident(incident.Id);

                await _realtime.IncidentChangedAsync(
                    incident.Id,
                    agentEvent.Id,
                    incident.Status.ToString(),
                    incident.Severity.ToString(),
                    cancellationToken);
            }

            var policy = await _policyEngine.CheckAsync(
                context,
                decision,
                cancellationToken);

            if (!policy.Allowed)
            {
                if (decision.RequiresHumanApproval)
                {
                    execution.MarkPendingApproval(policy.Reason);

                    await _audit.WriteAsync(
    "Agent",
    "AI Incident Response Agent",
    "ExecutionPendingApproval",
    "AgentExecution",
    execution.Id.ToString(),
    execution.CorrelationId,
    $$"""
    {
      "decision": "{{execution.Decision}}",
      "action": "{{execution.Action}}",
      "reason": "{{execution.ApprovalReason}}"
    }
    """,
    cancellationToken);

                    if (incident is not null)
                    {
                        incident.MarkActionPending();
                    }
                }
                else
                {
                    execution.MarkSkipped(policy.Reason);

                    if (incident is not null)
                    {
                        incident.Escalate();
                    }
                }

                agentEvent.MarkProcessed();

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await NotifyExecutionCompletedAsync(
                    execution,
                    agentEvent,
                    incident,
                    cancellationToken);

                return;
            }

            if (decision.RequiresHumanApproval)
            {
                execution.MarkPendingApproval(decision.Reason);

                if (incident is not null)
                {
                    incident.MarkActionPending();
                }

                agentEvent.MarkProcessed();

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await NotifyExecutionCompletedAsync(
                    execution,
                    agentEvent,
                    incident,
                    cancellationToken);

                return;
            }

            if (decision.Decision is AgentDecision.ObserveOnly or AgentDecision.SuggestAction)
            {
                execution.MarkSkipped(decision.Reason);

                if (incident is not null)
                {
                    incident.MarkActionPending();
                }

                agentEvent.MarkProcessed();

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await NotifyExecutionCompletedAsync(
                    execution,
                    agentEvent,
                    incident,
                    cancellationToken);

                return;
            }

            _logger.LogInformation(
    "Executing action. ExecutionId={ExecutionId}, Action={Action}",
    execution.Id,
    decision.Action);

            var actionResult = await _actionExecutor.ExecuteAsync(
                decision.Action,
                context,
                cancellationToken);

            _logger.LogInformation(
    "Action execution completed. ExecutionId={ExecutionId}, Action={Action}, Success={Success}",
    execution.Id,
    decision.Action,
    actionResult.Success);

            await _feedbackHandler.HandleAsync(
                context,
                decision,
                actionResult,
                cancellationToken);

            await _memoryService.UpdateMemoryAsync(
                context,
                actionResult,
                cancellationToken);

            if (actionResult.Success)
            {
                var alreadyLocked = await _actionLocks.ExistsAsync(
                       decision.Action,
                       context.Event.CorrelationId,
                       cancellationToken);

                if (!alreadyLocked)
                {
                    var actionLock = new AgentActionLock(
                        decision.Action,
                        context.Event.CorrelationId,
                        agentEvent.Id);

                    await _actionLocks.AddAsync(actionLock, cancellationToken);
                }

                execution.MarkSucceeded(actionResult.ResultJson);

                await _audit.WriteAsync(
    "Agent",
    "AI Incident Response Agent",
    "ExecutionSucceeded",
    "AgentExecution",
    execution.Id.ToString(),
    execution.CorrelationId,
    $$"""
    {
      "action": "{{execution.Action}}",
      "status": "{{execution.Status}}"
    }
    """,
    cancellationToken);

                if (incident is not null)
                {
                    incident.MarkActionExecuted();
                    incident.Resolve();
                }
            }
            else
            {
                if (execution.RetryCount < _retryOptions.MaxRetries)
                {
                    var nextRetryAtUtc = RetryBackoffCalculator.CalculateNextRetryUtc(
                        execution.RetryCount,
                        _retryOptions);

                    execution.ScheduleRetry(
                        actionResult.ErrorMessage,
                        nextRetryAtUtc);

                    await _audit.WriteAsync(
    "Agent",
    "AI Incident Response Agent",
    "ExecutionRetryScheduled",
    "AgentExecution",
    execution.Id.ToString(),
    execution.CorrelationId,
    $$"""
    {
      "retryCount": {{execution.RetryCount}},
      "nextRetryAtUtc": "{{execution.NextRetryAtUtc}}",
      "error": "{{execution.ErrorMessage}}"
    }
    """,
    cancellationToken);

                    if (incident is not null)
                    {
                        incident.MarkActionPending();
                    }
                    _logger.LogWarning(
    "Action failed. Retry scheduled. ExecutionId={ExecutionId}, RetryCount={RetryCount}, NextRetryAtUtc={NextRetryAtUtc}, Error={ErrorMessage}",
    execution.Id,
    execution.RetryCount,
    execution.NextRetryAtUtc,
    actionResult.ErrorMessage);
                }
                else
                {
                    execution.MarkFinalFailed(actionResult.ErrorMessage);
                    await _audit.WriteAsync(
    "Agent",
    "AI Incident Response Agent",
    "ExecutionFailed",
    "AgentExecution",
    execution.Id.ToString(),
    execution.CorrelationId,
    $$"""
    {
      "action": "{{execution.Action}}",
      "error": "{{execution.ErrorMessage}}"
    }
    """,
    cancellationToken);

                    if (incident is not null)
                    {
                        incident.Fail();
                    }
                    _logger.LogError(
    "Action failed permanently. ExecutionId={ExecutionId}, RetryCount={RetryCount}, Error={ErrorMessage}",
    execution.Id,
    execution.RetryCount,
    actionResult.ErrorMessage);
                }
            }

            agentEvent.MarkProcessed();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation(
                "Agent event processing completed. ExecutionId={ExecutionId}, Status={Status}, DurationMs={DurationMs}",
                execution.Id,
                execution.Status,
                stopwatch.ElapsedMilliseconds);


            await NotifyExecutionCompletedAsync(
                execution,
                agentEvent,
                incident,
                cancellationToken);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            execution.MarkFailed(ex.Message);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogError(
                ex,
                "Agent event processing failed. ExecutionId={ExecutionId}, DurationMs={DurationMs}",
                execution.Id,
                stopwatch.ElapsedMilliseconds);

            await _realtime.AgentExecutionCompletedAsync(
                execution.Id,
                agentEvent.Id,
                execution.Status.ToString(),
                execution.Action.ToString(),
                agentEvent.CorrelationId,
                cancellationToken);
        }
    }

    private async Task NotifyExecutionCompletedAsync(
        AgentExecution execution,
        AgentEvent agentEvent,
        Incident? incident,
        CancellationToken cancellationToken)
    {
        await _realtime.AgentExecutionCompletedAsync(
            execution.Id,
            agentEvent.Id,
            execution.Status.ToString(),
            execution.Action.ToString(),
            agentEvent.CorrelationId,
            cancellationToken);

        if (incident is not null)
        {
            await _realtime.IncidentChangedAsync(
                incident.Id,
                agentEvent.Id,
                incident.Status.ToString(),
                incident.Severity.ToString(),
                cancellationToken);
        }
    }
}
