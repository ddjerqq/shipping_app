using Domain.Abstractions;
using Domain.Aggregates;

namespace Domain.Events;

public sealed record UserDeletedAccount(UserId UserId) : IDomainEvent;