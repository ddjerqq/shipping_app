using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Common;
using Application.Services;
using Domain.Aggregates;
using Domain.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Stripe.Issuing;

namespace Infrastructure.Services;

public sealed class JwtGenerator : IJwtGenerator
{
    public const string CookieName = "authorization";
    private const string SecurityAlgorithm = SecurityAlgorithms.EcdsaSha256;

    public static readonly string ClaimsIssuer = "JWT__ISSUER".FromEnvRequired();
    public static readonly string ClaimsAudience = "JWT__AUDIENCE".FromEnvRequired();
    public static readonly TimeSpan Expiration = TimeSpan.Parse("JWT__EXPIRATION".FromEnvRequired());
    public static readonly string EncryptionKeyPassword = "JWT__ENCRYPTION_KEY_PASSWORD".FromEnvRequired();
    public static readonly string EncryptionKeyPath = "JWT__ENCRYPTION_KEY_PATH".FromEnvRequired();
    public static readonly string SigningKeyPath = "JWT__SIGNING_KEY_PATH".FromEnvRequired();

    private readonly JsonWebTokenHandler _handler;
    private readonly RsaSecurityKey _privateEncryptionKey;
    private readonly RsaSecurityKey _publicEncryptionKey;
    private readonly ECDsaSecurityKey _privateSigningKey;
    private readonly ECDsaSecurityKey _publicSigningKey;
    public readonly TokenValidationParameters TokenValidationParameters;

    public JwtGenerator()
    {
        var encryptionKey = RSA.Create();
        var encryptionKeyText = File.ReadAllText(EncryptionKeyPath);
        encryptionKey.ImportFromEncryptedPem(encryptionKeyText, Encoding.UTF8.GetBytes(EncryptionKeyPassword));
        
        var signingKeyText = File.ReadAllText(SigningKeyPath);
        var signingKey = ECDsa.Create();
        signingKey.ImportFromPem(signingKeyText);

        _handler = new JsonWebTokenHandler();
        
        _privateEncryptionKey = new RsaSecurityKey(encryptionKey);
        _publicEncryptionKey = new RsaSecurityKey(encryptionKey.ExportParameters(false));
        _privateSigningKey = new ECDsaSecurityKey(signingKey);
        _publicSigningKey = new ECDsaSecurityKey(ECDsa.Create(signingKey.ExportParameters(false)));

        TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = ClaimsIssuer,
            ValidAudience = ClaimsAudience,
            IssuerSigningKey = _publicSigningKey,
            TokenDecryptionKey = _privateEncryptionKey,
            RequireSignedTokens = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            // ValidAlgorithms = [SecurityAlgorithm],
            NameClaimType = ClaimsPrincipalExt.IdClaimType,
            RoleClaimType = ClaimsPrincipalExt.RoleClaimType,
        };
    }

    public string GenerateToken(IEnumerable<Claim> claims, TimeSpan? expiration = null)
    {
        expiration ??= Expiration;

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = ClaimsIssuer,
            Audience = ClaimsAudience,
            Claims = claims.ToDictionary(claim => claim.Type, object (claim) => claim.Value),
            Expires = DateTime.UtcNow.Add(expiration.Value),
            SigningCredentials = new SigningCredentials(_privateSigningKey, SecurityAlgorithms.EcdsaSha256),
            EncryptingCredentials = new EncryptingCredentials(_publicEncryptionKey, SecurityAlgorithms.RsaOAEP, SecurityAlgorithms.Aes256CbcHmacSha512),
        };

        return _handler.CreateToken(tokenDescriptor);
    }

    public string GenerateToken(User user, TimeSpan? expiration = null) => GenerateToken(user.GetAllClaims(), expiration);

    public async Task<IEnumerable<Claim>> TryValidateTokenAsync(string token)
    {
        try
        {
            var result = await _handler.ValidateTokenAsync(token, TokenValidationParameters);

            if (!result.IsValid)
                throw new SecurityTokenInvalidSignatureException("Invalid token signature");

            return result.ClaimsIdentity.Claims;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to validate token");
            return [];
        }
    }
}