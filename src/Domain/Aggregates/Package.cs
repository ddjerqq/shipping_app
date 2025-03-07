using System.Numerics;
using Domain.Abstractions;
using Domain.Entities;
using Domain.Events;
using Domain.ValueObjects;
using Generated;

namespace Domain.Aggregates;

[StrongId]
public sealed class Package(PackageId id) : AggregateRoot<PackageId>(id)
{
    private readonly List<PackageReceptionStatus> _statuses = [];

    // for ef
    public Package() : this(PackageId.New())
    {
    }

    // user ordered package and declared the package
    public required TrackingCode TrackingCode { get; init; }
    public required Category Category { get; set; }
    public required string Description { get; set; }
    public string? WebsiteAddress { get; set; }
    public required Money RetailPrice { get; set; }
    public required int ItemCount { get; set; }
    public required bool HouseDelivery { get; set; }
    public string? InvoiceFileKey { get; set; }
    public string? PictureFileKey { get; set; }

    public UserId? SenderId { get; init; }
    public User? Sender { get; init; }

    public required UserId OwnerId { get; init; }
    public required User Owner { get; init; } = null!;

    /// <summary>
    /// gets the dimensions in centimeters
    /// width, height and length
    /// </summary>
    public Vector3? Dimensions { get; private set; }

    /// <summary>
    /// Gets the weight in Grams
    /// </summary>
    public long? Weight { get; private set; }

    /// <summary>
    /// Set this when receiving the package in the warehouse
    /// </summary>
    public Money? PricePerKg { get; set; }

    public PackagePrice? Price => this is { Dimensions: {X: var x, Y: var y, Z: var z}, Weight: {} weight }
        ? new PackagePrice(x, y, z, weight, HouseDelivery, PricePerKg ?? new Money("USD", 0))
        : null;

    public bool IsPaid { get; private set; }
    public bool IsProhibited { get; private set; }

    // sent to destination
    public RaceId? RaceId { get; private set; }
    public Race? Race { get; private set; }

    public PackageReceptionStatus CurrentStatus => Statuses.Last();
    public required IEnumerable<PackageReceptionStatus> Statuses
    {
        get => _statuses.OrderBy(x => x.Status);
        init
        {
            var statuses = value.ToList();

            if (statuses.Count == 0)
                throw new InvalidOperationException("At least one status is required.");

            if (statuses.First().Status is not PackageStatus.Awaiting)
                throw new InvalidOperationException("First status must be 'Awaiting'.");

            _statuses = statuses;
        }
    }

    /// <summary>
    /// Creates the package when user is receiving online orders.
    /// </summary>
    public static Package CreateOnline(TrackingCode trackingCode, Category category, string description, string? websiteAddress, Money retailPrice, int itemCount, bool houseDelivery, User owner, Money? pricePerKg = null)
    {
        var packageId = PackageId.New();
        var package = new Package(packageId)
        {
            TrackingCode = trackingCode,
            Category = category,
            Description = description,
            WebsiteAddress = websiteAddress,
            RetailPrice = retailPrice,
            ItemCount = itemCount,
            HouseDelivery = houseDelivery,

            SenderId = null,
            Sender = null,
            OwnerId = owner.Id,
            Owner = owner,

            PricePerKg = pricePerKg,

            Statuses = [PackageReceptionStatus.Awaiting(packageId, DateTime.Now)],
        };

        owner.ReceivedPackages.Add(package);

        return package;
    }

    /// <summary>
    /// Creates the package when a user is sending the package to another
    /// </summary>
    public static Package CreatePersonal(Category category, User sender, User receiver, Money? pricePerKg = null)
    {
        var packageId = PackageId.New();
        var package = new Package(packageId)
        {
            TrackingCode = TrackingCode.New(),
            Category = category,
            Description = "User Send User",
            RetailPrice = new Money("USD", 1),
            WebsiteAddress = "sangoway.com",
            ItemCount = 1,
            HouseDelivery = false,

            SenderId = sender.Id,
            Sender = sender,

            OwnerId = receiver.Id,
            Owner = receiver,

            PricePerKg = pricePerKg,

            Statuses = [PackageReceptionStatus.Awaiting(packageId, DateTime.Now)],
        };

        sender.SentPackages.Add(package);
        receiver.ReceivedPackages.Add(package);

        return package;
    }

    private void UpdateStatus(PackageReceptionStatus receptionStatus)
    {
        if (IsProhibited)
            throw new InvalidOperationException("This package has prohibited content, it's status cannot be changed");

        if (CurrentStatus.Status + 1 != receptionStatus.Status)
            throw new InvalidOperationException($"This order ({Id}) is not in the correct status to be set to {receptionStatus}");

        if (CurrentStatus.Status >= receptionStatus.Status)
            throw new InvalidOperationException($"This order ({Id}) already has a status of type {receptionStatus}");

        _statuses.Add(receptionStatus);
    }

    /// <inheritdoc cref="PackageReceptionStatus.AtWarehouse"/>
    public void ArrivedAtWarehouse(User staff, Vector3 dimensions, long weightGrams, DateTime date, Money pricePerKg)
    {
        PricePerKg = pricePerKg;
        Dimensions = dimensions;
        Weight = weightGrams;
        UpdateStatus(PackageReceptionStatus.AtWarehouse(Id, staff.Id, date));
        AddDomainEvent(new PackageArrivedAtWarehouse(Id, date, staff.Id));
    }

    /// <inheritdoc cref="PackageReceptionStatus.InTransit"/>
    public void SentToDestination(User staff, Race race, DateTime date)
    {
        RaceId = race.Id;
        Race = race;

        race.AddPackage(this);

        UpdateStatus(PackageReceptionStatus.InTransit(Id, staff.Id, date));
        AddDomainEvent(new PackageSentToDestination(Id, race.Id, date, staff.Id));
    }

    /// <inheritdoc cref="PackageReceptionStatus.AtDestination"/>
    public void ArrivedAtDestination(User staff, DateTime date)
    {
        UpdateStatus(PackageReceptionStatus.AtDestination(Id, staff.Id, date));
        AddDomainEvent(new PackageArrivedAtDestination(Id, date, staff.Id));
    }

    /// <inheritdoc cref="PackageReceptionStatus.Delivered"/>
    public void Delivered(DateTime date)
    {
        UpdateStatus(PackageReceptionStatus.Delivered(Id, date));
        AddDomainEvent(new PackageDelivered(Id, date));
    }

    public void FlagAsProhibited()
    {
        IsProhibited = true;
        AddDomainEvent(new PackageIsDeemedProhibited(Id));
    }

    public void MarkPaid()
    {
        IsPaid = true;
    }
}