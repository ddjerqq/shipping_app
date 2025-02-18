using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[AllowAnonymous]
public sealed class CultureController : ControllerBase
{
    [HttpGet("set-culture")]
    public IActionResult SetCulture([FromQuery] string culture, [FromQuery] string redirectUri)
    {
        if (string.IsNullOrEmpty(culture))
            return BadRequest("empty culture");

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(
                new RequestCulture(culture)));

        return LocalRedirect(redirectUri);
    }
}