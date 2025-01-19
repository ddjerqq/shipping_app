using Domain.Abstractions;
using Domain.Aggregates;

namespace Domain.Events;

public sealed record PackageArrivedAtWarehouse(PackageId PackageId, UserId ReceiverId, DateTimeOffset Date) : IDomainEvent;