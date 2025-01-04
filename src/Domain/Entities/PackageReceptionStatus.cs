using Domain.Abstractions;
using Domain.Aggregates;
using Domain.ValueObjects;
using Generated;

namespace Domain.Entities;

[StrongId]
public sealed class PackageReceptionStatus(PackageReceptionStatusId id) : Entity<PackageReceptionStatusId>(id), IComparable<PackageReceptionStatus>
{
    public PackageStatus Status { get; private init; }
    public DateTimeOffset Date { get; private init; }

    public PackageId PackageId { get; private init; }
    public Package? Package { get; private init; }
    public bool UserIsNull => Status is PackageStatus.Awaiting or PackageStatus.Delivered;
    public UserId? UserId { get; private init; }
    public User? User { get; private init; }

    /// <summary>
    /// Creates a new reception status indicating that the package is awaiting arrival to warehouse.
    /// </summary>
    public static PackageReceptionStatus Awaiting(PackageId packageId, DateTimeOffset date) => new(PackageReceptionStatusId.New())
    {
        Status = PackageStatus.Awaiting,
        PackageId = packageId,
        Date = date,
    };

    /// <summary>
    /// Creates a new reception status indicating that the package has been received at the origin warehouse by the specified staff user.
    /// </summary>
    public static PackageReceptionStatus AtWarehouse(Package package, User receivedBy, DateTimeOffset date) => new(PackageReceptionStatusId.New())
    {
        Status = PackageStatus.InWarehouse,
        PackageId = package.Id,
        Package = package,
        UserId = receivedBy.Id,
        User = receivedBy,
        Date = date,
    };

    /// <summary>
    /// Creates a new reception status indicating that the package has been sent to the destination warehouse by the specified staff user.
    /// </summary>
    public static PackageReceptionStatus InTransit(Package package, User sentBy, DateTimeOffset date) => new(PackageReceptionStatusId.New())
    {
        Status = PackageStatus.InTransit,
        PackageId = package.Id,
        Package = package,
        UserId = sentBy.Id,
        User = sentBy,
        Date = date,
    };

    /// <summary>
    /// Creates a new reception status indicating that the package has arrived at the destination warehouse. and collected by the specified staff user.
    /// </summary>
    public static PackageReceptionStatus AtDestination(Package package, User receivedBy, DateTimeOffset date) => new(PackageReceptionStatusId.New())
    {
        Status = PackageStatus.Arrived,
        PackageId = package.Id,
        Package = package,
        UserId = receivedBy.Id,
        User = receivedBy,
        Date = date,
    };

    /// <summary>
    /// Creates a new reception status indicating that the package has been delivered to the recipient.
    /// </summary>
    public static PackageReceptionStatus Delivered(Package package, DateTimeOffset date) => new(PackageReceptionStatusId.New())
    {
        Status = PackageStatus.Delivered,
        PackageId = package.Id,
        Package = package,
        Date = date,
    };

    public int CompareTo(PackageReceptionStatus? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        return other is null ? 1 : Status.CompareTo(other.Status);
    }
}