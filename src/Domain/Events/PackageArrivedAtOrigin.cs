using Domain.Abstractions;
using Domain.Aggregates;

namespace Domain.Events;

public sealed record PackageArrivedAtOrigin(PackageId PackageId, UserId ReceivedById, DateTimeOffset Date) : IDomainEvent;