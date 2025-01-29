namespace Domain.Common;

public static class StringExt
{
    public static string Capitalize(this string str) => str[..1].ToUpper() + str[1..].ToLower();

    public static string Initials(this string name) => string.Join(string.Empty, name.Split(' ').Select(part => part[0]));

    public static string CapitalizeName(this string name) => string.Join(" ", name.Split(' ').Select(part => part.Capitalize()));

    public static string? FromEnv(this string key)
    {
        return Environment.GetEnvironmentVariable(key);
    }

    public static string FromEnv(this string key, string value)
    {
        return Environment.GetEnvironmentVariable(key) ?? value;
    }

    public static string FromEnvRequired(this string key)
    {
        return Environment.GetEnvironmentVariable(key)
               ?? throw new InvalidOperationException($"Environment variable not found: {key}");
    }
}