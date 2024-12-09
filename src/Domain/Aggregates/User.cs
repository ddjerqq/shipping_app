using Domain.Abstractions;
using Generated;
using Microsoft.AspNetCore.Identity;

namespace Domain.Aggregates;

[StrongId]
public sealed class User() : ApplicationUser
{
    [ProtectedPersonalData]
    public string PersonalId { get; init; }

    public ICollection<Package> Packages { get; init; } = [];
}