using System.Globalization;
using Application.Exceptions;
using Domain.Aggregates;
using Domain.ValueObjects;

namespace Application.Services;

public interface ICurrentUserAccessor
{
    public UserId? Id { get; }

    public Role? Role { get; }

    public TimeZoneInfo? TimeZoneInfo { get; }

    public CultureInfo? CultureInfo { get; }

    public DateTime ConvertTimeToUserTime(DateTime dateTime) => TimeZoneInfo.ConvertTime(dateTime, TimeZoneInfo ?? TimeZoneInfo.Utc);

    public Task<User?> TryGetCurrentUserAsync(CancellationToken ct = default);

    public async Task<User> GetCurrentUserAsync(CancellationToken ct = default) =>
        await TryGetCurrentUserAsync(ct) ?? throw new UnauthenticatedException("The user is not authenticated");
}