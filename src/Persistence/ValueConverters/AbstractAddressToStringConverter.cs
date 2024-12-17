using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Persistence.ValueConverters;

internal sealed class AbstractAddressToStringConverter()
    : ValueConverter<AbstractAddress, string>(
        to => AddressToString(to),
        from => AddressFromString(from))
{
    private static string AddressToString(AbstractAddress address) => address switch
    {
        NoAddress => "not-specified",
        AirportAddress airportAddress => $"airport {airportAddress.Country} {airportAddress.AirportCode}",
        ZipCodeAddress zipCodeAddress => $"zip {zipCodeAddress.Country} {zipCodeAddress.ZipCode}",
        FullAddress fullAddress => $"full {fullAddress.Country} {fullAddress.State} {fullAddress.City} {fullAddress.ZipCode} {fullAddress.Address}",
        _ => throw new ArgumentException("Address type not supported yet"),
    };

    private static AbstractAddress AddressFromString(string address) => address.Split(' ') switch
    {
        ["not-specified"] => new NoAddress(),
        ["airport", var country, var airportCode] => new AirportAddress(country, airportCode),
        ["zip", var country, var zipCode] => new ZipCodeAddress(country, int.Parse(zipCode)),
        ["full", var country, var state, var city, var zipCode, .. var rest] => new FullAddress(country, state, city, zipCode, string.Join(' ', rest)),
        _ => throw new ArgumentException("Address type not supported yet"),
    };
}