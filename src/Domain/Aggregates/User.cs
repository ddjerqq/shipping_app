using System.Globalization;
using Destructurama.Attributed;
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

    [LogMasked]
    public string? PersonalId { get; set; }
    
    public required string Username { get; init; }
    
    [LogMasked]
    public required string Email { get; init; }
    
    [LogMasked]
    public string? PhoneNumber { get; init; }

    [LogMasked]
    public string? HomeNumber { get; init; }

    [LogMasked]
    public AbstractAddress AddressInfo { get; set; } = new NoAddress();

    public CultureInfo CultureInfo { get; set; } = CultureInfo.InvariantCulture;
    
    public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Utc;

    public int RoomCode { get; init; } = Random.Shared.Next(1_000_000, 9_999_999);

    public bool EmailConfirmed { get; private set; }
    // this could be a Type later on but for now its okay.
    public bool NotifyBySms { get; set; }

    public Money Balance { get; private set; } = new("USD", 0);

    [LogMasked]
    public string? PasswordHash { get; private set; }
    public string SecurityStamp { get; private set; } = Guid.NewGuid().ToString();
    public string ConcurrencyStamp { get; private set; } = Guid.NewGuid().ToString();
    public DateTime? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }
    public ICollection<UserClaim> Claims { get; init; } = [];

    public ICollection<Package> SentPackages { get; init; } = [];
    public ICollection<Package> ReceivedPackages { get; init; } = [];
    public ICollection<UserLogin> Logins { get; init; } = [];
    public Role Role { get; init; } = Role.User;

    public void AddBalance(Money amount, PaymentMethod paymentMethod, object paymentSessionId)
    {
        if (amount.Currency != Balance.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies. Convert the currencies first");

        Balance += amount;
        AddDomainEvent(new UserBalanceTopUp(Id, amount, paymentMethod, paymentSessionId));
    }

    public void PayForPackage(Package package)
    {
        if (Balance < package.Price!.TotalPrice)
            throw new InvalidOperationException($"The current user does not have enough balance to pay for the package. Balance: {Balance.FormatedValue} - Price: {package.Price.TotalPrice.FormatedValue}");

        Balance -= package.Price!.TotalPrice;
        package.MarkPaid();

        AddDomainEvent(new UserPaidForPackage(package.Id));
    }

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