using Domain.Aggregates;
using Domain.ValueObjects;

namespace Application.Services;

public interface IPaymentService
{
    public Task<string> CreatePaymentUrl(User user, Money money, CancellationToken ct = default);
}