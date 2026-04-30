using AiIncidentResponseAgent.Domain.Memory;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiIncidentResponseAgent.Infrastructure.Persistence.Configurations;

public sealed class AgentMemoryConfiguration : IEntityTypeConfiguration<AgentMemory>
{
    public void Configure(EntityTypeBuilder<AgentMemory> builder)
    {
        builder.ToTable("agent_memory");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.EntityId)
            .HasColumnName("entity_id")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.EntityType)
            .HasColumnName("entity_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ContextJson)
            .HasColumnName("context")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.LastUpdatedAtUtc).HasColumnName("last_updated_at_utc").IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.HasIndex(x => new { x.EntityType, x.EntityId }).IsUnique();
        builder.HasIndex(x => x.LastUpdatedAtUtc);
    }
}