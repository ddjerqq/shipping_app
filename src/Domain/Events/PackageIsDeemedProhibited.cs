using Domain.Abstractions;
using Domain.Aggregates;

namespace Domain.Events;

public sealed record PackageIsDeemedProhibited(PackageId PackageId) : IDomainEvent;