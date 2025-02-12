using Domain.Abstractions;
using Domain.Aggregates;
using Domain.ValueObjects;

namespace Domain.Events;

public sealed record UserBalanceTopUp(UserId UserId, Money Amount, PaymentMethod PaymentMethod, object PaymentSessionId) : IDomainEvent;