namespace Domain.ValueObjects;

public abstract record AbstractAddress(string? Country = null);

public sealed record NoAddress : AbstractAddress;

[Obsolete("remove this safely later")]
public sealed record AirportAddress(string Country, string AirportCode) : AbstractAddress(Country);

[Obsolete("remove this safely later")]
public sealed record ZipCodeAddress(string Country, int ZipCode) : AbstractAddress(Country);

public sealed record UserHouseAddress(string Country, string State, string City, string ZipCode, string Address) : AbstractAddress(Country);