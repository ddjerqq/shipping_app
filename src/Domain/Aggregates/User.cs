using Domain.Abstractions;
using Domain.ValueObjects;
using Generated;
using Microsoft.AspNetCore.Identity;

namespace Domain.Aggregates;

[StrongId]
public sealed class User : ApplicationUser
{
    [ProtectedPersonalData]
    public required string PersonalId { get; init; }

    public required AbstractAddress AddressInfo { get; init; }

    public IEnumerable<Package> Packages { get; init; } = [];
}