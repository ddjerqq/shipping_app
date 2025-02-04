using Domain.Aggregates;
using Domain.ValueObjects;

namespace Application.Services;

public interface IPaymentService
{
    public Task<string> CreatePaymentUrl(User user, long amountInCents, Currency currency, CancellationToken ct = default);
}