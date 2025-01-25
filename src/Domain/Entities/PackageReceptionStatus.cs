using Domain.Abstractions;
using Domain.Aggregates;
using Domain.ValueObjects;
using Generated;

namespace Domain.Entities;

[StrongId]
public sealed class PackageReceptionStatus(PackageReceptionStatusId id) : Entity<PackageReceptionStatusId>(id), IComparable<PackageReceptionStatus>
{
    public PackageStatus Status { get; private init; }
    public DateTime Date { get; private init; }

    public PackageId PackageId { get; private init; }
    public bool UserIsNull => Status is PackageStatus.Awaiting or PackageStatus.Delivered;

    public User? Staff { get; private init; }
    public UserId? StaffId { get; private init; }

    /// <summary>
    /// Creates a new reception status indicating that the package is awaiting arrival to warehouse.
    /// </summary>
    public static PackageReceptionStatus Awaiting(PackageId packageId, DateTime date) => new(PackageReceptionStatusId.New())
    {
        Status = PackageStatus.Awaiting,
        PackageId = packageId,
        Date = date,
    };

    /// <summary>
    /// Creates a new reception status indicating that the package has been received at the origin warehouse by the specified staff user.
    /// </summary>
    public static PackageReceptionStatus AtWarehouse(PackageId packageId, UserId staffId, DateTime date) => new(PackageReceptionStatusId.New())
    {
        Status = PackageStatus.InWarehouse,
        PackageId = packageId,
        StaffId = staffId,
        Date = date,
    };

    /// <summary>
    /// Creates a new reception status indicating that the package has been sent to the destination warehouse by the specified staff user.
    /// </summary>
    public static PackageReceptionStatus InTransit(PackageId packageId, UserId staffId, DateTime date) => new(PackageReceptionStatusId.New())
    {
        Status = PackageStatus.InTransit,
        PackageId = packageId,
        StaffId = staffId,
        Date = date,
    };

    /// <summary>
    /// Creates a new reception status indicating that the package has arrived at the destination warehouse. and collected by the specified staff user.
    /// </summary>
    public static PackageReceptionStatus AtDestination(PackageId packageId, UserId staffId, DateTime date) => new(PackageReceptionStatusId.New())
    {
        Status = PackageStatus.Arrived,
        PackageId = packageId,
        StaffId = staffId,
        Date = date,
    };

    /// <summary>
    /// Creates a new reception status indicating that the package has been delivered to the recipient.
    /// </summary>
    public static PackageReceptionStatus Delivered(PackageId packageId, DateTime date) => new(PackageReceptionStatusId.New())
    {
        Status = PackageStatus.Delivered,
        PackageId = packageId,
        Date = date,
    };

    public int CompareTo(PackageReceptionStatus? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        return other is null ? 1 : Status.CompareTo(other.Status);
    }
}