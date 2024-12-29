using System.Numerics;
using Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

internal class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        builder.Property(x => x.TrackingCode).HasMaxLength(32);
        // category
        builder.Property(x => x.Description).HasMaxLength(256);
        builder.Property(x => x.WebsiteAddress).HasMaxLength(32);
        // retail price
        // house delivery
        builder.HasOne(package => package.Owner)
            .WithMany(user => user.Packages)
            .HasForeignKey(package => package.OwnerId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Property(x => x.Dimensions)
            .HasConversion(x => VecToString(x), x => StringToVec(x));
        // weight grams
        builder.Ignore(x => x.ShippingPrice);

        builder.HasOne(package => package.Race)
            .WithMany(race => race.Packages)
            .HasForeignKey(package => package.RaceId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.HasMany(package => package.Statuses)
            .WithOne(status => status.Package)
            .HasForeignKey(status => status.PackageId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }

    private static string? VecToString(Vector3? vec) => vec is { X: var x, Y: var y, Z: var z } ? $"{x}:{y}:{z}" : null;

    private static Vector3? StringToVec(string? vec) => vec?.Split(':') switch
    {
        null => null,
        [var x, var y, var z] => new Vector3(float.Parse(x), float.Parse(y), float.Parse(z)),
        _ => throw new FormatException("Vector3 must be in format 'X:Y:Z'"),
    };
}