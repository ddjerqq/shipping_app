using Domain.Aggregates;
using Domain.Entities;

namespace Domain.ValueObjects;

public sealed class PackageReceptionStatus : IComparable<PackageReceptionStatus>
{
    public PackageStatus Status { get; private init; }

    public PackageId PackageId { get; private init; }
    public Package? Package { get; private init; }
    public DateTimeOffset Date { get; private init; }

    public bool UserIsNull => Status is PackageStatus.Awaiting or PackageStatus.Delivered;
    public UserId? StaffId { get; private init; }
    public User? Staff { get; private init; }

    /// <summary>
    /// Creates a new reception status indicating that the package is awaiting arrival to warehouse.
    /// </summary>
    public static PackageReceptionStatus Awaiting(PackageId packageId, DateTimeOffset date) => new()
    {
        Status = PackageStatus.Awaiting,
        PackageId = packageId,
        Date = date,
    };

    /// <summary>
    /// Creates a new reception status indicating that the package has been received at the origin warehouse by the specified staff user.
    /// </summary>
    public static PackageReceptionStatus AtOrigin(Package package, User receivedBy, DateTimeOffset date) => new()
    {
        Status = PackageStatus.AtOrigin,
        PackageId = package.Id,
        Package = package,
        StaffId = receivedBy.Id,
        Staff = receivedBy,
        Date = date,
    };

    /// <summary>
    /// Creates a new reception status indicating that the package has been sent to the destination warehouse by the specified staff user.
    /// </summary>
    public static PackageReceptionStatus InTransit(Package package, User sentBy, DateTimeOffset date) => new()
    {
        Status = PackageStatus.InTransit,
        PackageId = package.Id,
        Package = package,
        StaffId = sentBy.Id,
        Staff = sentBy,
        Date = date,
    };

    /// <summary>
    /// Creates a new reception status indicating that the package has arrived at the destination warehouse. and collected by the specified staff user.
    /// </summary>
    public static PackageReceptionStatus AtDestination(Package package, User receivedBy, DateTimeOffset date) => new()
    {
        Status = PackageStatus.AtDestination,
        PackageId = package.Id,
        Package = package,
        StaffId = receivedBy.Id,
        Staff = receivedBy,
        Date = date,
    };

    /// <summary>
    /// Creates a new reception status indicating that the package has been delivered to the recipient.
    /// </summary>
    public static PackageReceptionStatus Delivered(Package package, DateTimeOffset date) => new()
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