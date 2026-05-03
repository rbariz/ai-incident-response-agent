using AiIncidentResponseAgent.Domain.Audit;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiIncidentResponseAgent.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ActorType)
            .HasColumnName("actor_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ActorName)
            .HasColumnName("actor_name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Action)
            .HasColumnName("action")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.EntityType)
            .HasColumnName("entity_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.EntityId)
            .HasColumnName("entity_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.DetailsJson)
            .HasColumnName("details_json")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.HasIndex(x => x.ActorName);
        builder.HasIndex(x => x.Action);
        builder.HasIndex(x => x.EntityType);
        builder.HasIndex(x => x.EntityId);
        builder.HasIndex(x => x.CorrelationId);
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}
