using System.Security.Claims;
using Domain.Abstractions;
using Domain.Aggregates;
using Generated;

namespace Domain.Entities;

[StrongId]
public sealed class UserClaim(UserClaimId id) : Entity<UserClaimId>(id)
{
    public required UserId UserId { get; init; }
    public required string Type { get; init; }
    public required string Value { get; init; }

    public static implicit operator Claim(UserClaim userClaim) => new(userClaim.Type, userClaim.Value);
}