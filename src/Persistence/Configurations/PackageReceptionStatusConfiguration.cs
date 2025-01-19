using Domain.Aggregates;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

internal class PackageReceptionStatusConfiguration : IEntityTypeConfiguration<PackageReceptionStatus>
{
    public void Configure(EntityTypeBuilder<PackageReceptionStatus> builder)
    {
        // status
        // date

        builder.HasOne<Package>()
            .WithMany(package => package.Statuses)
            .HasForeignKey(status => status.PackageId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Ignore(x => x.UserIsNull);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(package => package.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}