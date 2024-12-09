using Domain.Abstractions;
using Domain.Aggregates;
using Domain.Entities;

namespace Domain.Events;

// if not paid notify user to pay
// if no declaration exists on our end, notify the user?
public sealed record PackageArrivedAtDestination(PackageId PackageId, UserId ReceivedById, DateTimeOffset Date) : IDomainEvent;