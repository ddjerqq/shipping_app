namespace Presentation.Common;

public static class ConfigurationExt
{
    public static Dictionary<string, string?> GetSupportedCultures(this IConfiguration configuration) => configuration
        .GetSection("Cultures")
        .GetChildren()
        .ToDictionary(x => x.Key, x => x.Value);
}