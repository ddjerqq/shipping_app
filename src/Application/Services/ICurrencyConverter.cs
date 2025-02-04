using Domain.ValueObjects;

namespace Application.Services;

public interface ICurrencyConverter
{
    /// <summary>
    /// Converts the source money's currency and amount into the destination currency
    /// </summary>
    public Task<Money> ConvertToAsync(Money source, Currency destination, CancellationToken ct = default);
}