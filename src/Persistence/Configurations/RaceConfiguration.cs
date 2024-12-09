using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

internal class RaceConfiguration : IEntityTypeConfiguration<Race>
{
    public void Configure(EntityTypeBuilder<Race> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(32);
        builder.Property(x => x.Origin).HasMaxLength(3);
        builder.Property(x => x.Destination).HasMaxLength(3);

        builder.HasMany(race => race.Packages)
            .WithOne(package => package.Race)
            .HasForeignKey(package => package.RaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}