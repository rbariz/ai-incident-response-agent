using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AiIncidentResponseAgent.Domain.Events;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiIncidentResponseAgent.Infrastructure.Persistence.Configurations;

public sealed class AgentEventConfiguration : IEntityTypeConfiguration<AgentEvent>
{
    public void Configure(EntityTypeBuilder<AgentEvent> builder)
    {
        builder.ToTable("agent_events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Type).HasColumnName("type").HasConversion<int>().IsRequired();
        builder.Property(x => x.Source).HasColumnName("source").HasMaxLength(100).IsRequired();

        builder.Property(x => x.PayloadJson)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Processed).HasColumnName("processed").IsRequired();
        builder.Property(x => x.ProcessedAtUtc).HasColumnName("processed_at_utc");
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.HasIndex(x => x.Processed);
        builder.HasIndex(x => x.CorrelationId);
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}
