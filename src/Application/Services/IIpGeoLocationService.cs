using IPinfo.Models;

namespace Application.Services;

public interface IIpGeoLocationService
{
    public Task<IPResponse?> GetIpInfoAsync(string? ip, CancellationToken ct = default);
}