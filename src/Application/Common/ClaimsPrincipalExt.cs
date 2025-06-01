using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using Domain.Aggregates;
using Domain.ValueObjects;

namespace Application.Common;

public static class ClaimsPrincipalExt
{
    public const string IdClaimType = "sid";
    public const string PersonalIdClaimType = "pid";
    public const string UsernameClaimType = "name";
    public const string EmailClaimType = "email";
    public const string PhoneClaimType = "phone";
    public const string RoleClaimType = "role";
    public const string SecurityStampClaimType = "security_stamp";
    public const string RoomCodeClaimType = "room_code";
    public const string TimeZoneClaimType = "time_zone";
    public const string CultureClaimType = "time_zone";

    public static UserId? GetId(this ClaimsPrincipal principal) => UserId.TryParse(
        principal.Claims.FirstOrDefault(c => c.Type == IdClaimType)?.Value, null, out var id)
        ? id
        : null;

    public static string? GetPersonalId(this ClaimsPrincipal principal) => principal.Claims.FirstOrDefault(c => c.Type == PersonalIdClaimType)?.Value;
    public static string? GetUsername(this ClaimsPrincipal principal) => principal.Claims.FirstOrDefault(c => c.Type == UsernameClaimType)?.Value;
    public static string? GetEmail(this ClaimsPrincipal principal) => principal.Claims.FirstOrDefault(c => c.Type == EmailClaimType)?.Value;
    public static string? GetPhoneNumber(this ClaimsPrincipal principal) => principal.Claims.FirstOrDefault(c => c.Type == PhoneClaimType)?.Value;
    public static string? GetSecurityStamp(this ClaimsPrincipal principal) => principal.Claims.FirstOrDefault(c => c.Type == SecurityStampClaimType)?.Value;
    public static string? GetRoomCode(this ClaimsPrincipal principal) => principal.Claims.FirstOrDefault(c => c.Type == RoomCodeClaimType)?.Value;

    public static Role? GetRole(this ClaimsPrincipal principal) =>
        Enum.TryParse<Role>(principal.Claims.FirstOrDefault(c => c.Type == RoleClaimType)?.Value, out var role)
            ? role
            : null;

    public static TimeZoneInfo? GetTimeZone(this ClaimsPrincipal principal) =>
        principal.Claims.FirstOrDefault(c => c.Type == TimeZoneClaimType)?.Value is { } tz
            ? TimeZoneInfo.FindSystemTimeZoneById(tz)
            : null;

    public static CultureInfo GetCulture(this ClaimsPrincipal principal) =>
        principal.Claims.FirstOrDefault(c => c.Type == CultureClaimType)?.Value is { } id
            ? CultureInfo.GetCultureInfo(id)
            : CultureInfo.InvariantCulture;

    public static IEnumerable<Claim> GetAllClaims(this User user) =>
    [
        new(IdClaimType, user.Id.ToString()),
        new(PersonalIdClaimType, user.PersonalId ?? "00000000000"),
        new(UsernameClaimType, user.Username),
        new(EmailClaimType, user.Email),
        new(PhoneClaimType, user.PhoneNumber),
        new(RoleClaimType, user.Role.ToString()),
        new(SecurityStampClaimType, user.SecurityStamp),
        new(RoomCodeClaimType, user.RoomCode.ToString()),
        new(TimeZoneClaimType, user.TimeZone.Id),
    ];

    public static string GetDefaultAvatar(string? username = null)
    {
        username ??= RandomNumberGenerator.GetHexString(5);
        username = UrlEncoder.Default.Encode(username);
        return $"https://api.dicebear.com/9.x/glass/svg?backgroundType=gradientLinear&scale=50&seed={username}";
    }
}