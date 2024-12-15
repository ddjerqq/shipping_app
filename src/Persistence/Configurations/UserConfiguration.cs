using Domain.Aggregates;
using EntityFrameworkCore.DataProtection.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.ValueConverters;

namespace Persistence.Configurations;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(x => x.PersonalId)
            .IsEncryptedQueryable(isUnique: true)
            .HasMaxLength(11);

        builder.HasIndex(x => x.PersonalId)
            .IsUnique();

        builder.Property(x => x.AddressInfo)
            .HasConversion<AbstractAddressToStringConverter>()
            .IsEncrypted();
    }

}