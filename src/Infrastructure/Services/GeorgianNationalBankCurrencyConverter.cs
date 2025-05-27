using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Application.Services;
using Domain.ValueObjects;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Infrastructure.Services;

internal sealed record BaseApiResponse(List<ApiCurrency> Currencies);

internal sealed record ApiCurrency(
    [property: JsonPropertyName("code")] Currency Currency,
    [property: JsonPropertyName("quantity")] int Quantity,
    [property: JsonPropertyName("rate")] float Rate);

internal sealed class GeorgianNationalBankCurrencyConverter(HttpClient http, IDistributedCache cache) : ICurrencyConverter
{
    private const string ApiUrl = "https://nbg.gov.ge/gw/api/ct/monetarypolicy/currencies/en/json";

    private async Task<IEnumerable<ApiCurrency>> FetchCurrencies(CancellationToken ct = default)
    {
        var cachedCurrencies = await cache.GetStringAsync("currencies", ct);
        if (cachedCurrencies is not null)
            return JsonConvert.DeserializeObject<IEnumerable<ApiCurrency>>(cachedCurrencies) ?? [];

        var response = await http.GetFromJsonAsync<List<BaseApiResponse>>(ApiUrl, ct);
        var currencies = response?[0].Currencies ?? [];

        await cache.SetStringAsync("currencies", JsonConvert.SerializeObject(currencies), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
        }, ct);

        return currencies;
    }

    /// <summary>
    /// Converts the source money to destination currency via GEL
    /// </summary>
    public async Task<Money> ConvertToAsync(Money source, Currency destination, CancellationToken ct = default)
    {
        if (source.Currency == destination)
            return source;

        var currencies = (await FetchCurrencies(ct)).ToList();

        var sourceCurrency = currencies.First(x => x.Currency == source.Currency);
        var amountConvertedToGel = (float)source.Amount / 100 * sourceCurrency.Rate / sourceCurrency.Quantity;
        var destinationCurrency = currencies.First(x => x.Currency == destination);
        var amountConvertedToDestination = amountConvertedToGel * destinationCurrency.Quantity / destinationCurrency.Rate;

        return new Money(destination, (long)(amountConvertedToDestination * 100));
    }
}