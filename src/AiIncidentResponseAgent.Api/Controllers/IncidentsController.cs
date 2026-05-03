using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Contracts.Common;
using AiIncidentResponseAgent.Contracts.Ops;
using AiIncidentResponseAgent.Domain.Incidents;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiIncidentResponseAgent.Api.Controllers;

[Authorize(Policy = "CanViewOps")]
[ApiController]
[Route("api/incidents")]
public sealed class IncidentsController : ControllerBase
{
    private readonly IIncidentRepository _incidents;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRealtimeNotifier _realtime;

    public IncidentsController(IIncidentRepository incidents, IUnitOfWork unitOfWork, IRealtimeNotifier realtime)
    {
        _incidents = incidents;
        _unitOfWork = unitOfWork;
        _realtime = realtime;
    }



    [HttpGet]
    public async Task<ActionResult<PagedResponse<IncidentResponse>>> GetLatest(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 50,
    CancellationToken cancellationToken = default)
    {
        var incidents = await _incidents.GetPagedAsync(
            page,
            pageSize,
            cancellationToken);

        return Ok(new PagedResponse<IncidentResponse>
        {
            Items = incidents.Items.Select(ToResponse).ToList(),
            Page = incidents.Page,
            PageSize = incidents.PageSize,
            TotalCount = incidents.TotalCount
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<IncidentResponse>> GetById(
    Guid id,
    CancellationToken cancellationToken)
    {
        var incident = await _incidents.GetByIdAsync(id, cancellationToken);

        if (incident is null || incident.IsArchived)
            return NotFound();

        return Ok(ToResponse(incident));
    }


    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanManageApprovals")]
    public async Task<ActionResult<IncidentResponse>> Update(
    Guid id,
    [FromBody] UpdateIncidentRequest request,
    CancellationToken cancellationToken)
    {
        var incident = await _incidents.GetByIdAsync(id, cancellationToken);

        if (incident is null || incident.IsArchived)
            return NotFound();

        if (!Enum.TryParse<IncidentSeverity>(request.Severity, true, out var severity))
            return BadRequest("Invalid severity.");

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Title is required.");

        incident.UpdateDetails(
            request.Title,
            request.Description,
            severity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _realtime.IncidentChangedAsync(
            incident.Id,
            incident.AgentEventId,
            incident.Status.ToString(),
            incident.Severity.ToString(),
            cancellationToken);

        return Ok(ToResponse(incident));
    }

    [HttpPost("{id:guid}/resolve")]
    [Authorize(Policy = "CanManageApprovals")]
    public async Task<ActionResult<IncidentResponse>> Resolve(
    Guid id,
    CancellationToken cancellationToken)
    {
        var incident = await _incidents.GetByIdAsync(id, cancellationToken);

        if (incident is null || incident.IsArchived)
            return NotFound();

        incident.Resolve();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _realtime.IncidentChangedAsync(
            incident.Id,
            incident.AgentEventId,
            incident.Status.ToString(),
            incident.Severity.ToString(),
            cancellationToken);

        return Ok(ToResponse(incident));
    }


    [HttpPost("{id:guid}/reopen")]
    [Authorize(Policy = "CanManageApprovals")]
    public async Task<ActionResult<IncidentResponse>> Reopen(
    Guid id,
    CancellationToken cancellationToken)
    {
        var incident = await _incidents.GetByIdAsync(id, cancellationToken);

        if (incident is null || incident.IsArchived)
            return NotFound();

        incident.Reopen();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _realtime.IncidentChangedAsync(
            incident.Id,
            incident.AgentEventId,
            incident.Status.ToString(),
            incident.Severity.ToString(),
            cancellationToken);

        return Ok(ToResponse(incident));
    }


    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CanAdmin")]
    public async Task<IActionResult> Archive(
    Guid id,
    CancellationToken cancellationToken)
    {
        var incident = await _incidents.GetByIdAsync(id, cancellationToken);

        if (incident is null || incident.IsArchived)
            return NotFound();

        incident.Archive();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _realtime.IncidentChangedAsync(
            incident.Id,
            incident.AgentEventId,
            incident.Status.ToString(),
            incident.Severity.ToString(),
            cancellationToken);

        return NoContent();
    }



    private static IncidentResponse ToResponse(Incident x) => new()
    {
        Id = x.Id,
        AgentEventId = x.AgentEventId,
        Title = x.Title,
        Description = x.Description,
        Severity = x.Severity.ToString(),
        Status = x.Status.ToString(),
        CreatedAtUtc = x.CreatedAtUtc,
        ResolvedAtUtc = x.ResolvedAtUtc,
        IsArchived = x.IsArchived
    };
}
