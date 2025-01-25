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
            .HasForeignKey(packageReceptionStatus => packageReceptionStatus.PackageId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Ignore(x => x.UserIsNull);

        builder.HasOne<User>(packageReceptionStatus => packageReceptionStatus.Staff)
            .WithMany()
            .HasForeignKey(packageReceptionStatus => packageReceptionStatus.StaffId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}