using System.Text.RegularExpressions;
using Destructurama.Attributed;
using Domain.Abstractions;
using Domain.Aggregates;
using Generated;

namespace Domain.Entities;

[StrongId]
public sealed partial class UserLogin(UserLoginId id) : Entity<UserLoginId>(id)
{
    public User User { get; init; } = null!;
    public UserId UserId { get; init; }
    
    [LogMasked]
    public required string UserAgent { get; init; }
    
    [LogMasked]
    public required string Location { get; init; }
    
    [LogMasked]
    public required string IpAddress { get; init; }
    
    public required DateTime LastActive { get; set; }

    public string DeviceType
    {
        get
        {
            if (string.IsNullOrEmpty(UserAgent))
                return "unknown";

            if (MobileRegex().IsMatch(UserAgent))
                return "mobile";

            if (TabletRegex().IsMatch(UserAgent))
                return "tablet";

            return "desktop";
        }
    }

    [GeneratedRegex("(iphone|ipod|android.*mobile|windows phone|blackberry)", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex MobileRegex();

    [GeneratedRegex("(ipad|android(?!.*mobile)|tablet)", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex TabletRegex();
}