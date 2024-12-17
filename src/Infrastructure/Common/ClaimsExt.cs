using System.Security.Claims;

namespace Infrastructure.Common;

public static class ClaimsExt
{
    public static bool HasAllKeys(this IEnumerable<Claim> claims, params string[] keys) => claims.All(c => keys.Contains(c.Type));

    public static string? FindFirstValue(this IEnumerable<Claim> claims, string type) => claims.FirstOrDefault(c => c.Type == type)?.Value;
}