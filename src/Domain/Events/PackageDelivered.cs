using Domain.Abstractions;
using Domain.Aggregates;

namespace Domain.Events;

public sealed record PackageDelivered(PackageId PackageId, DateTimeOffset Date) : IDomainEvent;