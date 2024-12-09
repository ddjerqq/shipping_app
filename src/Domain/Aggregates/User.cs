using Domain.Abstractions;
using Domain.Entities;
using Generated;
using Microsoft.AspNetCore.Identity;

namespace Domain.Aggregates;

[StrongId]
public sealed class User : ApplicationUser
{
    [ProtectedPersonalData]
    public required string PersonalId { get; init; }

    public ICollection<Package> Packages { get; init; } = [];
}