using Domain.Aggregates;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

internal class UserClaimConfiguration : IEntityTypeConfiguration<UserClaim>
{
    public void Configure(EntityTypeBuilder<UserClaim> builder)
    {
        builder.Property(x => x.Type)
            .HasMaxLength(32);

        builder.Property(x => x.Value)
            .HasMaxLength(64);

        builder.HasIndex(x => new { x.UserId, x.Type })
            .IsUnique();

        builder.HasOne<User>()
            .WithMany(u => u.Claims)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}