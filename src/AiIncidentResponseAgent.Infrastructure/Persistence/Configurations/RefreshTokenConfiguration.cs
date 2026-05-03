using AiIncidentResponseAgent.Domain.Auth;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiIncidentResponseAgent.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .HasColumnName("token_hash")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.ExpiresAtUtc)
            .HasColumnName("expires_at_utc")
            .IsRequired();

        builder.Property(x => x.IsRevoked)
            .HasColumnName("is_revoked")
            .IsRequired();

        builder.Property(x => x.RevokedAtUtc)
            .HasColumnName("revoked_at_utc");

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ExpiresAtUtc);
    }
}
