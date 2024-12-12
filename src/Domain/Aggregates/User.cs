using Domain.Abstractions;
using Generated;
using Microsoft.AspNetCore.Identity;

namespace Domain.Aggregates;

[StrongId]
public sealed class User : ApplicationUser
{
    [ProtectedPersonalData]
    public required string PersonalId { get; init; }

    public IEnumerable<Package> Packages { get; init; } = [];
}