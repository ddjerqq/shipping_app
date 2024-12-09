using Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(x => x.PersonalId)
            .HasMaxLength(11);

        builder.HasIndex(x => x.PersonalId)
            .IsUnique();

        // builder.Property<CultureInfo>()
        //     .HasConversion(culture => culture.Name, name => new CultureInfo(name))
        //     .HasMaxLength(6);

        // builder.Property<IEdition>()
        //     .HasConversion(e => EditionToString(e), s => StringToEdition(s))
        //     .HasMaxLength(11);

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