namespace Domain.Common;

public static class StringExt
{
    public static string Capitalize(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        if (str.Length == 1)
        {
            return str.ToUpper();
        }

        return str[..1].ToUpper() + str[1..].ToLower();
    }

    public static string CapitalizeName(this string name)
    {
        return string.IsNullOrEmpty(name) ? string.Empty :
            string.Join(" ", name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(part => part.Capitalize()));
    }

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