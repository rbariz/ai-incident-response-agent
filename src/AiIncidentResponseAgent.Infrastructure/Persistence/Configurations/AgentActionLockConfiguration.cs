using AiIncidentResponseAgent.Domain.Actions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiIncidentResponseAgent.Infrastructure.Persistence.Configurations;

public sealed class AgentActionLockConfiguration : IEntityTypeConfiguration<AgentActionLock>
{
    public void Configure(EntityTypeBuilder<AgentActionLock> builder)
    {
        builder.ToTable("agent_action_locks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Action)
            .HasColumnName("action")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.AgentEventId)
            .HasColumnName("agent_event_id")
            .IsRequired();

        builder.Property(x => x.LockedAtUtc)
            .HasColumnName("locked_at_utc")
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.HasIndex(x => new { x.Action, x.CorrelationId }).IsUnique();
        builder.HasIndex(x => x.AgentEventId);
        builder.HasIndex(x => x.LockedAtUtc);
    }
}
