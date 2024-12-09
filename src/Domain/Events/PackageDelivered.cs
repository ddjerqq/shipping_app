using Domain.Abstractions;
using Domain.Aggregates;
using Domain.Entities;

namespace Domain.Events;

public sealed record PackageDelivered(PackageId PackageId, DateTimeOffset Date) : IDomainEvent;