using Domain.Aggregates;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

[Obsolete("i just dont like this either")]
public sealed class IdentityUserAccessor(IdentityRedirectManager redirectManager)
{
    public async Task<User> GetRequiredUserAsync(HttpContext context)
    {
        // var user = await userManager.GetUserAsync(context.User);
        //
        // if (user is null)
        // {
        //     redirectManager.RedirectToWithStatus("Account/InvalidUser", $"Error: Unable to load user with ID '{userManager.GetUserId(context.User)}'.", context);
        // }
        //
        // return user;
        return null;
    }
}