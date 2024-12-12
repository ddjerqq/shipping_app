using Domain.Aggregates;
using EntityFrameworkCore.DataProtection.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
    }

    // private static string EditionToString(IEdition edition) => edition switch
    // {
    //     OrdinalEdition ordinal => $"{ordinal.Number}",
    //     SeasonalEdition seasonal => $"{Enum.GetName(seasonal.Season)} {seasonal.Year}",
    //     _ => throw new ArgumentException("Edition type not supported yet"),
    // };
    //
    // private static IEdition StringToEdition(string edition) => edition.Split(' ') switch
    // {
    //     [var season, var year] => new SeasonalEdition((Season)Enum.Parse(typeof(Season), season), int.Parse(year)),
    //     [var number] => new OrdinalEdition(int.Parse(number)),
    //     _ => throw new ArgumentException("Edition type not supported yet"),
    // };
}