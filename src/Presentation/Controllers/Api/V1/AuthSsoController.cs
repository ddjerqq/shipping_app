using System.Security.Claims;
using Application.Cqrs.Users.Commands;
using Domain.Common;
using Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Presentation.Controllers.Api.V1;

[ApiController]
[Route("api/v1/auth/sso")]
public sealed class AuthSsoController(ILogger<AuthSsoController> logger, IMediator mediator) : ControllerBase
{
    private static string WebAppDomain => "WEB_APP__DOMAIN".FromEnvRequired();

    /// <summary>
    /// Start a new challenge for the selected SSO provider.
    /// </summary>
    /// <param name="provider">The provider, must be one of: "google", "microsoft", "github"</param>
    /// <param name="returnUrl"></param>
    /// <returns>ChallengeResponse for the selected provider</returns>
    /// <exception cref="ArgumentException">When the provider is not known</exception>
    /// <exception cref="InvalidOperationException">SsoCallbackTemplate is not present in the configuration</exception>
    [AllowAnonymous]
    [HttpGet("{provider}")]
    public IActionResult SsoChallenge([FromRoute] string provider, [FromQuery] string? returnUrl)
    {
        var schemeName = provider.ToLowerInvariant() switch
        {
            "google" => "Google",
            _ => throw new ArgumentException($"Unknown provider: {provider}"),
        };

        var redirectUrl = $"https://{WebAppDomain}/api/v1/auth/sso/{schemeName.ToLowerInvariant()}/callback";
        if (!string.IsNullOrWhiteSpace(returnUrl))
            redirectUrl = QueryHelpers.AddQueryString(redirectUrl, "returnUrl", returnUrl);

        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

        return Challenge(properties, schemeName);
    }

    /// <summary>
    /// This endpoint just authorizes with google, and extracts the email and the name that we need from the claims, and passes it down to HandleSso
    /// </summary>
    [AllowAnonymous]
    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleResponse([FromQuery(Name = "returnUrl")] string? returnUrl, CancellationToken ct = default)
    {
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!result.Succeeded)
            return Unauthorized();

        var email = result.Principal.FindFirstValue(ClaimTypes.Email);
        var fullName = result.Principal.FindFirstValue(ClaimTypes.Name);

        var request = new AuthorizeWithSsoCommand(email!, fullName!);

        return await HandleSso(request, returnUrl, ct);
    }

    private async Task<IActionResult> HandleSso(AuthorizeWithSsoCommand request, string? returnUrl, CancellationToken ct = default)
    {
        try
        {
            var (token, user) = await mediator.Send(request, ct);
            var expirationDuration = TimeSpan.Parse("JWT__EXPIRATION".FromEnvRequired());

            Response.Cookies.Append("authorization", token, new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.Add(expirationDuration)
            });

            var url = user.Role switch
            {
                Role.User => "/user_dashboard",
                Role.Staff => "/staff_dashboard",
                Role.Admin => "/admin_dashboard",
                _ => "/",
            };
            return Redirect(url);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error authenticating user");
            return BadRequest(ex.Message);
        }
    }
}