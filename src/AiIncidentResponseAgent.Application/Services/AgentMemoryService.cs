using AiIncidentResponseAgent.Application.Abstractions;
using AiIncidentResponseAgent.Application.Abstractions.Repositories;
using AiIncidentResponseAgent.Application.Models;
using AiIncidentResponseAgent.Domain.Memory;

namespace AiIncidentResponseAgent.Application.Services;

public sealed class AgentMemoryService : IAgentMemoryService
{
    private readonly IAgentMemoryRepository _memoryRepository;

    public AgentMemoryService(IAgentMemoryRepository memoryRepository)
    {
        _memoryRepository = memoryRepository;
    }

    public async Task<string> LoadMemoryAsync(
        AgentContext context,
        CancellationToken cancellationToken = default)
    {
        var entityId = context.Event.CorrelationId;
        var entityType = context.Event.Type.ToString();

        var memory = await _memoryRepository.GetByEntityAsync(
            entityType,
            entityId,
            cancellationToken);

        return memory?.ContextJson ?? "{}";
    }

    public async Task UpdateMemoryAsync(
        AgentContext context,
        AgentActionResult actionResult,
        CancellationToken cancellationToken = default)
    {
        var entityId = context.Event.CorrelationId;
        var entityType = context.Event.Type.ToString();

        var memory = await _memoryRepository.GetByEntityAsync(
            entityType,
            entityId,
            cancellationToken);

        var json = $$"""
        {
          "lastEventId": "{{context.Event.Id}}",
          "lastActionSuccess": {{actionResult.Success.ToString().ToLowerInvariant()}},
          "lastResult": {{actionResult.ResultJson}}
        }
        """;

        if (memory is null)
        {
            memory = new AgentMemory(entityId, entityType, json);
            await _memoryRepository.AddAsync(memory, cancellationToken);
            return;
        }

        memory.UpdateContext(json);
    }
}
