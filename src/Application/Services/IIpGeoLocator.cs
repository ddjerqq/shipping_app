using IPinfo.Models;

namespace Application.Services;

public interface IIpGeoLocator
{
    public Task<IPResponse?> GetIpInfoAsync(string? ip, CancellationToken ct = default);
}