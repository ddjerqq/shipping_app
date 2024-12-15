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
        builder.Property(x => x.Origin)
            .HasConversion<AbstractAddressToStringConverter>()
            .IsEncrypted();

        builder.Property(x => x.Destination)
            .HasConversion<AbstractAddressToStringConverter>()
            .IsEncrypted();

        builder.HasMany(race => race.Packages)
            .WithOne(package => package.Race)
            .HasForeignKey(package => package.RaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}