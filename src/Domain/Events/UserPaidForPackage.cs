using Domain.Abstractions;
using Domain.Aggregates;

namespace Domain.Events;

public sealed record UserPaidForPackage(PackageId PackageId) : IDomainEvent;