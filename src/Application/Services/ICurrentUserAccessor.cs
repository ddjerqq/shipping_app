using Application.Exceptions;
using Domain.Aggregates;
using Domain.Entities;

namespace Application.Services;

public interface ICurrentUserAccessor
{
    public UserId? Id { get; }

    public string? PersonalId { get; }
    public string? Username { get; }
    public string? Email { get; }
    public string? PhoneNumber { get; }
    public string? SecurityStamp { get; }
    public IEnumerable<RoleId> RoleIds { get; }

    public Task<User?> TryGetCurrentUserAsync(CancellationToken ct = default);

    public async Task<User> GetCurrentUserAsync(CancellationToken ct = default) =>
        await TryGetCurrentUserAsync(ct) ?? throw new UnauthenticatedException("The user is not authenticated");
}