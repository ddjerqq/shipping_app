using System.Numerics;
using Domain.Abstractions;
using Domain.Common;
using Domain.Entities;
using Domain.Events;
using Domain.ValueObjects;
using Generated;

namespace Domain.Aggregates;

[StrongId]
public sealed class Package(PackageId id) : AggregateRoot<PackageId>(id)
{
    public const decimal PricePerKg = 8;
    private readonly List<PackageReceptionStatus> _statuses = [];

    // for ef
    public Package() : this(PackageId.New())
    {
    }

    // user ordered package and declared the package
    public required TrackingCode TrackingCode { get; init; }
    public required Category Category { get; set; }
    public required string Description { get; set; }
    public required string WebsiteAddress { get; set; }
    public required Money RetailPrice { get; set; }
    public required int ItemCount { get; set; }
    public required bool HouseDelivery { get; init; }
    public string? InvoiceFileKey { get; set; }
    public string? PictureFileKey { get; set; }

    public required UserId OwnerId { get; init; }
    public required User Owner { get; init; } = null!;

    // arrived at origin.
    // scan the barcode, go to an address like - /track/{CODE}
    // set the dimensions and weight of the package

    /// <summary>
    /// gets the dimensions in centimeters
    /// </summary>
    public Vector3? Dimensions { get; private set; }
    public float? WeightGrams { get; private set; }

    public decimal ShippingPrice => this.GetTotalPrice();

    // TODO payment method and receipt here. comes from stripe api?
    public bool IsPaid { get; init; }

    // sent to destination
    public RaceId? RaceId { get; private set; }
    public Race? Race { get; private set; }

    public PackageReceptionStatus CurrentStatus => _statuses.Last();
    public required IEnumerable<PackageReceptionStatus> Statuses
    {
        get => _statuses;
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
    /// Creates the package, generating the tracking code if null, and adds it to the user's packages.
    /// </summary>
    public static Package Create(TrackingCode? trackingCode, Category category, string description, string websiteAddress, Money retailPrice, int itemCount, bool houseDelivery, User owner)
    {
        var packageId = PackageId.New();
        var package = new Package(packageId)
        {
            TrackingCode = trackingCode ?? TrackingCode.New(),
            Category = category,
            Description = description,
            WebsiteAddress = websiteAddress,
            RetailPrice = retailPrice,
            ItemCount = itemCount,
            HouseDelivery = houseDelivery,

            OwnerId = owner.Id,
            Owner = owner,

            Statuses = [PackageReceptionStatus.Awaiting(packageId, DateTimeOffset.Now)],
        };

        owner.Packages.Add(package);

        return package;
    }

    private void UpdateStatus(PackageReceptionStatus receptionStatus)
    {
        if (_statuses.Last().Status >= receptionStatus.Status)
            throw new InvalidOperationException($"This order ({Id}) already has a status of type {receptionStatus}");

        _statuses.Add(receptionStatus);
    }

    /// <inheritdoc cref="PackageReceptionStatus.AtOrigin"/>
    public void ArrivedAtOrigin(User receivedBy, Vector3 dimensions, float weightGrams, DateTimeOffset date)
    {
        Dimensions = dimensions;
        WeightGrams = weightGrams;
        UpdateStatus(PackageReceptionStatus.AtOrigin(this, receivedBy, date));
        AddDomainEvent(new PackageArrivedAtOrigin(Id, receivedBy.Id, date));
    }

    /// <inheritdoc cref="PackageReceptionStatus.InTransit"/>
    public void SentToDestination(User sentBy, Race race, DateTimeOffset date)
    {
        RaceId = race.Id;
        Race = race;

        race.Packages.Add(this);

        UpdateStatus(PackageReceptionStatus.InTransit(this, sentBy, date));
        AddDomainEvent(new PackageSentToDestination(Id, sentBy.Id, race.Id, date));
    }

    /// <inheritdoc cref="PackageReceptionStatus.AtDestination"/>
    public void ArrivedAtDestination(User receivedBy, DateTimeOffset date)
    {
        UpdateStatus(PackageReceptionStatus.AtDestination(this, receivedBy, date));
        AddDomainEvent(new PackageArrivedAtDestination(Id, receivedBy.Id, date));
    }

    /// <inheritdoc cref="PackageReceptionStatus.Delivered"/>
    public void Delivered(DateTimeOffset date)
    {
        UpdateStatus(PackageReceptionStatus.Delivered(this, date));
        AddDomainEvent(new PackageDelivered(Id, date));
    }
}