using Domain.Aggregates;

namespace Application.Services;

public interface IUserVerificationTokenGenerator
{
    public string GenerateToken(User user, string purpose);

    /// <summary>
    /// Validates that the jwt is valid and the token's purpose matches
    /// </summary>
    /// <remarks>
    /// Please note it is your responsibility to validate the security_stamp and the sid
    /// </remarks>
    public (string Purpose, DateTimeOffset ValidUntil, string SecurityStamp, UserId UserId)? ValidateToken(string purpose, string token);
}