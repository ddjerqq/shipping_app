using Domain.Abstractions;
using Domain.Aggregates;
using Domain.Entities;

namespace Domain.Events;

public sealed record PackageSentToDestination(PackageId PackageId, UserId SentById, RaceId RaceId, DateTimeOffset Date) : IDomainEvent;