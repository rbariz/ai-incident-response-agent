using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Application.Models;
using AiIncidentResponseAgent.Contracts.Common;
using AiIncidentResponseAgent.Contracts.Ops;
using AiIncidentResponseAgent.Domain.Actions;
using AiIncidentResponseAgent.Domain.Executions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiIncidentResponseAgent.Api.Controllers;

[Authorize(Policy = "CanViewOps")]
[ApiController]
[Route("api/agent-executions")]
public sealed class AgentExecutionsController : ControllerBase
{
    private readonly IAgentExecutionRepository _executions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRealtimeNotifier _realtime;

    private readonly IAgentEventRepository _events;
    private readonly IAgentActionExecutor _actionExecutor;
    private readonly IAgentFeedbackHandler _feedbackHandler;
    private readonly IAgentMemoryService _memoryService;
    private readonly IAgentActionLockRepository _actionLocks;

    public AgentExecutionsController(IAgentExecutionRepository executions, IUnitOfWork unitOfWork, IRealtimeNotifier realtime, IAgentEventRepository events, IAgentActionExecutor actionExecutor, IAgentFeedbackHandler feedbackHandler, IAgentMemoryService memoryService, IAgentActionLockRepository actionLocks)
    {
        _executions = executions;
        _unitOfWork = unitOfWork;
        _realtime = realtime;
        _events = events;
        _actionExecutor = actionExecutor;
        _feedbackHandler = feedbackHandler;
        _memoryService = memoryService;
        _actionLocks = actionLocks;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<AgentExecutionResponse>>> GetLatest(
    [FromQuery] string? correlationId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 50,
    CancellationToken cancellationToken = default)
    {
        var executions = await _executions.GetPagedAsync(
            correlationId,
            page,
            pageSize,
            cancellationToken);

        return Ok(new PagedResponse<AgentExecutionResponse>
        {
            Items = executions.Items.Select(ToResponse).ToList(),
            Page = executions.Page,
            PageSize = executions.PageSize,
            TotalCount = executions.TotalCount
        });
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "CanManageApprovals")]
    public async Task<IActionResult> Approve(
    Guid id,
    [FromBody] ApproveExecutionRequest request,
    CancellationToken cancellationToken)
    {
        var execution = await _executions.GetByIdAsync(id, cancellationToken);

        if (execution is null)
        {
            return NotFound();
        }

        var agentEvent = await _events.GetByIdAsync(
            execution.AgentEventId,
            cancellationToken);

        if (agentEvent is null)
        {
            return BadRequest("Related agent event was not found.");
        }

        try
        {
            var reason = string.IsNullOrWhiteSpace(request.Reason)
                ? "Approved by operator."
                : request.Reason.Trim();

            execution.MarkApprovedAndRunning(reason);

            await _realtime.AgentExecutionApprovalChangedAsync(
                execution.Id,
                execution.Status.ToString(),
                execution.ApprovalReason,
                cancellationToken);

            var context = new AgentContext
            {
                Event = agentEvent,
                MemoryJson = "{}",
                Lang = agentEvent.Lang
            };

            var alreadyLocked = await _actionLocks.ExistsAsync(
                execution.Action,
                agentEvent.CorrelationId,
                cancellationToken);

            if (alreadyLocked)
            {
                execution.MarkFinalFailed(
                    $"Action {execution.Action} already locked for correlation id {agentEvent.CorrelationId}.");

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _realtime.AgentExecutionCompletedAsync(
                    execution.Id,
                    execution.AgentEventId,
                    execution.Status.ToString(),
                    execution.Action.ToString(),
                    execution.CorrelationId,
                    cancellationToken);

                return Conflict(ToResponse(execution));
            }

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
                    Reason = execution.ApprovalReason
                },
                actionResult,
                cancellationToken);

            await _memoryService.UpdateMemoryAsync(
                context,
                actionResult,
                cancellationToken);

            if (actionResult.Success)
            {
                var lockAlreadyCreated = await _actionLocks.ExistsAsync(
                    execution.Action,
                    agentEvent.CorrelationId,
                    cancellationToken);

                if (!lockAlreadyCreated)
                {
                    var actionLock = new AgentActionLock(
                        execution.Action,
                        agentEvent.CorrelationId,
                        agentEvent.Id);

                    await _actionLocks.AddAsync(actionLock, cancellationToken);
                }

                execution.MarkSucceeded(actionResult.ResultJson);
            }
            else
            {
                execution.MarkFinalFailed(actionResult.ErrorMessage);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _realtime.AgentExecutionCompletedAsync(
                execution.Id,
                execution.AgentEventId,
                execution.Status.ToString(),
                execution.Action.ToString(),
                execution.CorrelationId,
                cancellationToken);

            return Ok(ToResponse(execution));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = "CanManageApprovals")]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectExecutionRequest request,
        CancellationToken cancellationToken)
    {
        var execution = await _executions.GetByIdAsync(id, cancellationToken);

        if (execution is null)
        {
            return NotFound();
        }

        try
        {
            execution.Reject(string.IsNullOrWhiteSpace(request.Reason)
                ? "Rejected by operator."
                : request.Reason.Trim());

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _realtime.AgentExecutionApprovalChangedAsync(
                execution.Id,
                execution.Status.ToString(),
                execution.ApprovalReason,
                cancellationToken);


            await _realtime.AgentExecutionCompletedAsync(
                    execution.Id,
                    execution.AgentEventId,
                    execution.Status.ToString(),
                    execution.Action.ToString(),
                    execution.CorrelationId,
                    cancellationToken);  //?

            return Ok(ToResponse(execution));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    private static AgentExecutionResponse ToResponse(AgentExecution x) => new()
    {
        Id = x.Id,
        AgentEventId = x.AgentEventId,
        IncidentId = x.IncidentId,
        CorrelationId = x.CorrelationId,
        Status = x.Status.ToString(),
        Decision = x.Decision.ToString(),
        Action = x.Action.ToString(),
        AnalysisProvider = x.AnalysisProvider,
        AnalysisSummary = x.AnalysisSummary,
        ConfidenceScore = x.ConfidenceScore,
        ResultJson = x.ResultJson,
        ErrorMessage = x.ErrorMessage,
        RetryCount = x.RetryCount,
        CreatedAtUtc = x.CreatedAtUtc,
        StartedAtUtc = x.StartedAtUtc,
        CompletedAtUtc = x.CompletedAtUtc,
        AnalysisLanguage = x.AnalysisLanguage,
        AnalysisSummaryFr = x.AnalysisSummaryFr,
        AnalysisSummaryEn = x.AnalysisSummaryEn,
        ApprovalReason = x.ApprovalReason,
        ApprovedAtUtc = x.ApprovedAtUtc,
        RejectedAtUtc = x.RejectedAtUtc,
        NextRetryAtUtc = x.NextRetryAtUtc,
        LastRetryAtUtc = x.LastRetryAtUtc
    };
}
