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
    public const decimal PricePerKg = 8;
    private readonly List<PackageReceptionStatus> _statuses = [];

    public required AbstractAddress Origin { get; init; }
    public required AbstractAddress Destination { get; init; }

    // user ordered package and declared the package
    public required TrackingCode TrackingCode { get; init; }
    public required Category Category { get; init; }
    public required string Description { get; init; }
    public required WebAddress WebsiteAddress { get; init; }
    public required Money RetailPrice { get; init; }
    public required bool HouseDelivery { get; init; }

    public required UserId OwnerId { get; init; }
    public required User Owner { get; init; } = null!;

    // arrived at origin.
    // scan the barcode, go to an address like - /track/{CODE}
    // set the dimensions and weight of the package
    public Vector3? Dimensions { get; private set; }
    public double? WeightGrams { get; private set; }
    public decimal ShippingPrice => (decimal)(WeightGrams ?? 0) / 1000 * PricePerKg;

    // TODO payment method and receipt here. comes from stripe api?
    public bool IsPaid { get; init; }

    // sent to destination
    public RaceId? RaceId { get; private set; }
    public Race? Race { get; private set; }

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

    public static Package Create(AbstractAddress origin, AbstractAddress destination, TrackingCode? trackingCode, Category category, string description, WebAddress websiteAddress, Money retailPrice, bool houseDelivery, User sender)
    {
        var packageId = PackageId.New();
        return new Package(packageId)
        {
            Origin = origin,
            Destination = destination,

            TrackingCode = trackingCode ?? TrackingCode.New(),
            Category = category,
            Description = description,
            WebsiteAddress = websiteAddress,
            RetailPrice = retailPrice,
            HouseDelivery = houseDelivery,

            OwnerId = sender.Id,
            Owner = sender,

            Statuses = [PackageReceptionStatus.Awaiting(packageId, DateTimeOffset.Now)],
        };
    }

    private void UpdateStatus(PackageReceptionStatus receptionStatus)
    {
        if (_statuses.Last().Status >= receptionStatus.Status)
            throw new InvalidOperationException($"This order ({Id}) already has a status of type {receptionStatus}");

        _statuses.Add(receptionStatus);
    }

    /// <inheritdoc cref="PackageReceptionStatus.AtOrigin"/>
    public void ArrivedAtOrigin(User receivedBy, Vector3 dimensions, double weightGrams, DateTimeOffset date)
    {
        Dimensions = dimensions;
        WeightGrams = weightGrams;
        UpdateStatus(PackageReceptionStatus.AtOrigin(this, receivedBy, date));
        AddDomainEvent(new PackageArrivedAtOrigin(Id, receivedBy.Id, date));
    }

    /// <inheritdoc cref="PackageReceptionStatus.InTransit"/>
    public void SentToDestination(User sentBy, Race race, DateTimeOffset date)
    {
        if (race.Origin != Origin || race.Destination != Destination)
            throw new InvalidOperationException("The race origin and destination dont match the package");

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