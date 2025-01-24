using Domain.Entities;
using EntityFrameworkCore.DataProtection.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.ValueConverters;

namespace Persistence.Configurations;

internal class RaceConfiguration : IEntityTypeConfiguration<Race>
{
    public void Configure(EntityTypeBuilder<Race> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(32);
        builder.HasIndex(x => x.Name).IsUnique();

        builder.Property(x => x.Origin).HasMaxLength(4);
        builder.Property(x => x.Destination).HasMaxLength(4);

        builder.HasMany(race => race.Packages)
            .WithOne(package => package.Race)
            .HasForeignKey(package => package.RaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(x => x.QualifiedName);
    }
}