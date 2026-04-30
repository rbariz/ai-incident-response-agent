using AiIncidentResponseAgent.Domain.Executions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiIncidentResponseAgent.Infrastructure.Persistence.Configurations;

public sealed class AgentExecutionConfiguration : IEntityTypeConfiguration<AgentExecution>
{
    public void Configure(EntityTypeBuilder<AgentExecution> builder)
    {
        builder.ToTable("agent_executions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.AgentEventId).HasColumnName("agent_event_id").IsRequired();
        builder.Property(x => x.IncidentId).HasColumnName("incident_id");

        builder.Property(x => x.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Status).HasColumnName("status").HasConversion<int>().IsRequired();
        builder.Property(x => x.Decision).HasColumnName("decision").HasConversion<int>().IsRequired();
        builder.Property(x => x.Action).HasColumnName("action").HasConversion<int>().IsRequired();

        builder.Property(x => x.AnalysisSummary)
            .HasColumnName("analysis_summary")
            .HasMaxLength(4000);

        builder.Property(x => x.ConfidenceScore)
            .HasColumnName("confidence_score")
            .HasPrecision(5, 4);

        builder.Property(x => x.ResultJson)
            .HasColumnName("result")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(4000);

        builder.Property(x => x.RetryCount).HasColumnName("retry_count").IsRequired();
        builder.Property(x => x.StartedAtUtc).HasColumnName("started_at_utc");
        builder.Property(x => x.CompletedAtUtc).HasColumnName("completed_at_utc");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.HasIndex(x => x.IdempotencyKey).IsUnique();
        builder.HasIndex(x => x.AgentEventId);
        builder.HasIndex(x => x.IncidentId);
        builder.HasIndex(x => x.CorrelationId);
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}
