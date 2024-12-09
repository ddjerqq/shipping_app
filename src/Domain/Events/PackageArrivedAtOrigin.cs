using Domain.Abstractions;
using Domain.Aggregates;
using Domain.Entities;

namespace Domain.Events;

public sealed record PackageArrivedAtOrigin(PackageId PackageId, UserId ReceivedById, DateTimeOffset Date) : IDomainEvent;