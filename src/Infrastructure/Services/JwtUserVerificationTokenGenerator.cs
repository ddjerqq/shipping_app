using System.Security.Claims;
using Application.Services;
using Domain.Aggregates;
using Infrastructure.Common;

namespace Infrastructure.Services;

public sealed class JwtUserVerificationTokenGenerator(IJwtGenerator jwtGenerator) : IUserVerificationTokenGenerator
{    public string GenerateToken(User user, string purpose)
     {
         Claim[] claims =
         [
             new("purpose", purpose),
             new("security_stamp", user.SecurityStamp),
             new("sid", user.Id.ToString()),
         ];
 
         return jwtGenerator.GenerateToken(claims, TimeSpan.FromMinutes(5));
     }


    public async Task<(string Purpose, string SecurityStamp, UserId UserId)?> ValidateTokenAsync(string purpose, string token)
    {
        var claims = (await jwtGenerator.TryValidateTokenAsync(token)).ToList();

        if (claims.Count == 0)
            return null;

        if (!claims.HasAllKeys("purpose", "security_stamp", "sid"))
            return null;

        var claimPurpose = claims.FindFirstValue("purpose")!;
        var claimsSecurityStamp = claims.FindFirstValue("security_stamp")!;
        var claimsSid = UserId.Parse(claims.FindFirstValue("sid")!);

        if (claimPurpose == purpose)
            return (claimPurpose, claimsSecurityStamp, claimsSid);

        return null;
    }
}