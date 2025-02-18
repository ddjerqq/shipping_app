using System.Globalization;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[AllowAnonymous]
public sealed class CultureController(ICurrentUserAccessor currentUserAccessor, IAppDbContext dbContext) : ControllerBase
{
    [HttpGet("set_culture")]
    public async Task<IActionResult> SetCulture([FromQuery] string culture, [FromQuery] string redirectUri, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(culture))
            return BadRequest("empty culture");

        var currentUser = await currentUserAccessor.TryGetCurrentUserAsync(ct);
        if (currentUser is not null)
        {
            currentUser.CultureInfo = CultureInfo.GetCultureInfo(culture);
            await dbContext.SaveChangesAsync(ct);
        }

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(
                new RequestCulture(culture)));

        return LocalRedirect(redirectUri);
    }
}