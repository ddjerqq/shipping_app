using System.Globalization;
using Domain.Abstractions;
using Domain.Attributes;
using Domain.Entities;
using Domain.Events;
using Domain.ValueObjects;
using Generated;

namespace Domain.Aggregates;

[StrongId]
[SoftDelete]
public sealed class User(UserId id) : AggregateRoot<UserId>(id)
{
    public const int MaxAccessFailedCount = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(5);

    public required string PersonalId { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string PhoneNumber { get; init; }
    public AbstractAddress AddressInfo { get; set; } = new NoAddress();
    public CultureInfo CultureInfo { get; init; } = CultureInfo.InvariantCulture;
    public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Utc;

    public int RoomCode { get; init; } = Random.Shared.Next(1_000_000, 9_999_999);

    public bool EmailConfirmed { get; private set; }
    // this could be a Type later on but for now its okay.
    public bool NotifyBySms { get; set; }

    public string PasswordHash { get; private set; } = null!;
    public string SecurityStamp { get; private set; } = Guid.NewGuid().ToString();
    public string ConcurrencyStamp { get; private set; } = Guid.NewGuid().ToString();
    public DateTime? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }

    public ICollection<Package> Packages { get; init; } = [];
    public ICollection<UserClaim> Claims { get; init; } = [];
    public ICollection<UserLogin> Logins { get; init; } = [];
    public Role Role { get; init; } = Role.User;

    public void SetPassword(string newPassword, bool isInitial = false)
    {
        SecurityStamp = Guid.NewGuid().ToString();
        PasswordHash = BC.EnhancedHashPassword(newPassword);

        if (!isInitial)
            AddDomainEvent(new UserResetPassword(Id));
    }

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
        AddDomainEvent(new UserConfirmedEmail(Id));
    }

    public void AddLogin(UserLogin login)
    {
        Logins.Add(login);
        AddDomainEvent(new UserLoggedInFromNewDevice(Id, login.Id));
    }
}