using System.Globalization;
using Domain.Abstractions;
using Domain.Entities;
using Domain.Events;
using Domain.ValueObjects;
using Generated;

namespace Domain.Aggregates;

[StrongId]
public sealed class User(UserId id) : AggregateRoot<UserId>(id)
{
    public const int MaxAccessFailedCount = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(5);

    public required string PersonalId { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
    public AbstractAddress? AddressInfo { get; init; } = default!;
    public CultureInfo CultureInfo { get; init; } = CultureInfo.InvariantCulture;
    public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Utc;

    public bool EmailConfirmed { get; init; }

    public string PasswordHash { get; private set; } = default!;
    public string SecurityStamp { get; private set; } = Guid.NewGuid().ToString();
    public string ConcurrencyStamp { get; private set; } = Guid.NewGuid().ToString();
    public DateTimeOffset? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }

    public IEnumerable<Package> Packages { get; init; } = [];
    public ICollection<UserClaim> Claims { get; init; } = [];
    public ICollection<UserLogin> Logins { get; init; } = [];
    public ICollection<UserRole> Roles { get; init; } = [];

    public void SetPassword(string newPassword, bool isInitial = false)
    {
        SecurityStamp = Guid.NewGuid().ToString();
        PasswordHash = BC.EnhancedHashPassword(newPassword);

        if (!isInitial)
            AddDomainEvent(new UserResetPassword(Id));
    }
}