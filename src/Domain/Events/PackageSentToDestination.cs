using Domain.Abstractions;
using Domain.Aggregates;
using Domain.Entities;

namespace Domain.Events;

public sealed record PackageSentToDestination(PackageId PackageId, RaceId RaceId, DateTime Date, UserId StaffId) : IDomainEvent;