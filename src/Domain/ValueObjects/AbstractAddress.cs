namespace Domain.ValueObjects;

public abstract record AbstractAddress;

public sealed record NoAddress : AbstractAddress;
public sealed record FullAddress(string Country, string State, string City, string ZipCode, string Address) : AbstractAddress;

public static class AbstractAddressExt
{
    public static string AddressToString(this AbstractAddress address) => address switch
    {
        NoAddress => "not-specified",
        FullAddress fullAddress => $"full {fullAddress.Country} {fullAddress.State} {fullAddress.City} {fullAddress.ZipCode} {fullAddress.Address}",
        _ => throw new ArgumentException("Address type not supported yet"),
    };

    public static AbstractAddress AddressFromString(string address) => address.Split(' ') switch
    {
        ["not-specified"] => new NoAddress(),
        ["full", var country, var state, var city, var zipCode, .. var rest] => new FullAddress(country, state, city, zipCode, string.Join(' ', rest)),
        _ => throw new ArgumentException("Address type not supported yet"),
    };
}