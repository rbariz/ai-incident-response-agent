using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Application.Models;
using AiIncidentResponseAgent.Domain.Actions;
using AiIncidentResponseAgent.Domain.Incidents;

using Microsoft.Extensions.Options;

namespace AiIncidentResponseAgent.Application.Services;

public sealed class AgentRetryProcessor : IAgentRetryProcessor
{
    private readonly IAgentExecutionRepository _executions;
    private readonly IAgentEventRepository _events;
    private readonly IIncidentRepository _incidents;
    private readonly IAgentActionExecutor _actionExecutor;
    private readonly IAgentFeedbackHandler _feedbackHandler;
    private readonly IAgentMemoryService _memoryService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRealtimeNotifier _realtime;
    private readonly RetryOptions _retryOptions;
    private readonly IAgentActionLockRepository _actionLocks;

    public AgentRetryProcessor(
        IAgentExecutionRepository executions,
        IAgentEventRepository events,
        IIncidentRepository incidents,
        IAgentActionExecutor actionExecutor,
        IAgentFeedbackHandler feedbackHandler,
        IAgentMemoryService memoryService,
        IUnitOfWork unitOfWork,
        IRealtimeNotifier realtime,
        IOptions<RetryOptions> retryOptions,
        IAgentActionLockRepository actionLocks)
    {
        _executions = executions;
        _events = events;
        _incidents = incidents;
        _actionExecutor = actionExecutor;
        _feedbackHandler = feedbackHandler;
        _memoryService = memoryService;
        _unitOfWork = unitOfWork;
        _realtime = realtime;
        _retryOptions = retryOptions.Value;
        _actionLocks = actionLocks;
    }

    public async Task ProcessRetriesAsync(
        int take,
        CancellationToken cancellationToken = default)
    {
        var due = await _executions.GetRetryDueAsync(
            take,
            DateTime.UtcNow,
            cancellationToken);

        foreach (var execution in due)
        {
            await ProcessRetryAsync(execution, cancellationToken);
        }
    }

    private async Task ProcessRetryAsync(
        Domain.Executions.AgentExecution execution,
        CancellationToken cancellationToken)
    {
        var agentEvent = await _events.GetByIdAsync(
            execution.AgentEventId,
            cancellationToken);

        if (agentEvent is null)
        {
            execution.MarkFinalFailed("Related agent event not found for retry.");
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return;
        }

        Incident? incident = null;

        if (execution.IncidentId is not null)
        {
            incident = await _incidents.GetByIdAsync(
                execution.IncidentId.Value,
                cancellationToken);
        }

        execution.StartRetry();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _realtime.AgentExecutionStartedAsync(
            execution.Id,
            execution.AgentEventId,
            execution.CorrelationId,
            cancellationToken);

        var context = new AgentContext
        {
            Event = agentEvent,
            MemoryJson = "{}",
            Lang = agentEvent.Lang
        };

        var actionResult = await _actionExecutor.ExecuteAsync(
            execution.Action,
            context,
            cancellationToken);

        await _feedbackHandler.HandleAsync(
            context,
            new AgentDecisionResult
            {
                Decision = execution.Decision,
                Action = execution.Action,
                Reason = "Retry execution."
            },
            actionResult,
            cancellationToken);

        await _memoryService.UpdateMemoryAsync(
            context,
            actionResult,
            cancellationToken);

        if (actionResult.Success)
        {
            var alreadyLocked = await _actionLocks.ExistsAsync(
                execution.Action,
                agentEvent.CorrelationId,
                cancellationToken);

            if (!alreadyLocked)
            {
                var actionLock = new AgentActionLock(
                    execution.Action,
                    agentEvent.CorrelationId,
                    agentEvent.Id);

                await _actionLocks.AddAsync(actionLock, cancellationToken);
            }

            execution.MarkSucceeded(actionResult.ResultJson);

            if (incident is not null)
            {
                incident.MarkActionExecuted();
                incident.Resolve();
            }
        }
        else if (execution.RetryCount < _retryOptions.MaxRetries)
        {
            var nextRetryAtUtc = RetryBackoffCalculator.CalculateNextRetryUtc(
                execution.RetryCount,
                _retryOptions);

            execution.ScheduleRetry(actionResult.ErrorMessage, nextRetryAtUtc);

            if (incident is not null)
            {
                incident.MarkActionPending();
            }
        }
        else
        {
            execution.MarkFinalFailed(actionResult.ErrorMessage);

            if (incident is not null)
            {
                incident.Fail();
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _realtime.AgentExecutionCompletedAsync(
            execution.Id,
            execution.AgentEventId,
            execution.Status.ToString(),
            execution.Action.ToString(),
            execution.CorrelationId,
            cancellationToken);

        if (incident is not null)
        {
            await _realtime.IncidentChangedAsync(
                incident.Id,
                incident.AgentEventId,
                incident.Status.ToString(),
                incident.Severity.ToString(),
                cancellationToken);
        }
    }
}