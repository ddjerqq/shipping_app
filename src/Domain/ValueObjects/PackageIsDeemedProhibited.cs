using Domain.Abstractions;
using Domain.Aggregates;

namespace Domain.ValueObjects;

public sealed record PackageIsDeemedProhibited(PackageId PackageId) : IDomainEvent;