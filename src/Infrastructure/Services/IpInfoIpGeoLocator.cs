using Application.Services;
using Domain.Common;
using IPinfo;
using IPinfo.Cache;
using IPinfo.Models;

namespace Infrastructure.Services;

public sealed class IpInfoIpGeoLocator : IIpGeoLocator
{
    private readonly IPinfoClient _client;

    public IpInfoIpGeoLocator()
    {
        var token = "IPINFO__API_KEY".FromEnvRequired();

        _client = new IPinfoClient.Builder()
            .AccessToken(token)
            .Cache(new CacheWrapper(cacheConfig => cacheConfig
                .CacheMaxMbs(2)
                .CacheTtl(2 * 60 * 60 * 24)))
            .Build();
    }

    public Task<IPResponse?> GetIpInfoAsync(string? ip, CancellationToken ct = default) => _client.IPApi.GetDetailsAsync(ip, ct);
}