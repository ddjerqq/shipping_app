using Domain.Abstractions;
using Domain.Aggregates;

namespace Domain.Events;

public sealed record UserAddedByAdmin(UserId UserId, string Password) : IDomainEvent;
