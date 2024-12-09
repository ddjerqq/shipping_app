using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Encodings.Web;

namespace Application.Common;

public static class ClaimsPrincipalExt
{
    public const string IdClaimType = "sid";
    public const string UsernameClaimType = "name";
    public const string EmailClaimType = "email";
    public const string AvatarClaimType = "avatar";
    public const string RoleClaimType = "role";
    public const string SecurityStampClaimType = "security_stamp";

    public static string? GetId(this ClaimsPrincipal principal) => principal.Claims.FirstOrDefault(c => c.Type == IdClaimType)?.Value;
    public static string? GetUsername(this ClaimsPrincipal principal) => principal.Claims.FirstOrDefault(c => c.Type == UsernameClaimType)?.Value;
    public static string? GetEmail(this ClaimsPrincipal principal) => principal.Claims.FirstOrDefault(c => c.Type == EmailClaimType)?.Value;
    public static string? GetAvatar(this ClaimsPrincipal principal) => principal.Claims.FirstOrDefault(c => c.Type == AvatarClaimType)?.Value;
    public static string? GetRole(this ClaimsPrincipal principal) => principal.Claims.FirstOrDefault(c => c.Type == RoleClaimType)?.Value;
    public static string? GetSecurityStamp(this ClaimsPrincipal principal) => principal.Claims.FirstOrDefault(c => c.Type == SecurityStampClaimType)?.Value;

    public static string GetDefaultAvatar(string? username = default)
    {
        username ??= RandomNumberGenerator.GetHexString(5);
        username = UrlEncoder.Default.Encode(username);
        return $"https://api.dicebear.com/9.x/glass/svg?backgroundType=gradientLinear&scale=50&seed={username}";
    }

}