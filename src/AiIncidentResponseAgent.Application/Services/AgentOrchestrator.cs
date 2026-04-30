using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Application.Models;
using AiIncidentResponseAgent.Domain.Actions;
using AiIncidentResponseAgent.Domain.Executions;
using AiIncidentResponseAgent.Domain.Incidents;

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
        IAgentActionLockRepository actionLocks)
    {
        _events = events;
        _executions = executions;
        _incidents = incidents;
        _analyzer = analyzer;
        _decisionEngine = decisionEngine;
        _policyEngine = policyEngine;
        _actionExecutor = actionExecutor;
        _feedbackHandler = feedbackHandler;
        _memoryService = memoryService;
        _unitOfWork = unitOfWork;
        _actionLocks = actionLocks;
    }

    public async Task ProcessEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var agentEvent = await _events.GetByIdAsync(eventId, cancellationToken);

        if (agentEvent is null)
        {
            return;
        }

        var idempotencyKey = $"action-event:{agentEvent.Id}";

        var existingExecution = await _executions.GetByIdempotencyKeyAsync(
            idempotencyKey,
            cancellationToken);

        if (existingExecution is not null)
        {
            return;
        }

        var execution = new AgentExecution(
            agentEvent.Id,
            idempotencyKey,
            agentEvent.CorrelationId);

        await _executions.AddAsync(execution, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        execution.Start();

        try
        {
            var initialContext = new AgentContext
            {
                Event = agentEvent
            };

            var memoryJson = await _memoryService.LoadMemoryAsync(
                initialContext,
                cancellationToken);

            var context = new AgentContext
            {
                Event = agentEvent,
                MemoryJson = memoryJson
            };

            var analysis = await _analyzer.AnalyzeAsync(context, cancellationToken);

            var decision = await _decisionEngine.DecideAsync(
                context,
                analysis,
                cancellationToken);

            execution.SetDecision(
                decision.Decision,
                decision.Action,
                analysis.Summary,
                analysis.ConfidenceScore);

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
            }

            var policy = await _policyEngine.CheckAsync(
                context,
                decision,
                cancellationToken);

            if (!policy.Allowed)
            {
                execution.MarkSkipped(policy.Reason);

                if (incident is not null)
                {
                    incident.Escalate();
                }

                agentEvent.MarkProcessed();

                await _unitOfWork.SaveChangesAsync(cancellationToken);
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
                return;
            }


            var actionLock = new AgentActionLock(
                    decision.Action,
                    context.Event.CorrelationId,
                    agentEvent.Id);

            await _actionLocks.AddAsync(actionLock, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var actionResult = await _actionExecutor.ExecuteAsync(
                decision.Action,
                context,
                cancellationToken);

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
                execution.MarkSucceeded(actionResult.ResultJson);

                if (incident is not null)
                {
                    incident.MarkActionExecuted();
                    incident.Resolve();
                }
            }
            else
            {
                execution.MarkFailed(actionResult.ErrorMessage);

                if (incident is not null)
                {
                    incident.Fail();
                }
            }

            agentEvent.MarkProcessed();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            execution.MarkFailed(ex.Message);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
