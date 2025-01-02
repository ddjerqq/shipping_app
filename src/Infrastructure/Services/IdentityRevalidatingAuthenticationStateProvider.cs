using System.Security.Claims;
using Application.Common;
using Application.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public sealed class IdentityRevalidatingAuthenticationStateProvider(
    ILoggerFactory loggerFactory,
    IServiceScopeFactory scopeFactory)
    : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(3);

    protected override async Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUserAccessor>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        var userId = currentUser.Id;
        if (userId is null)
            return false;

        // we don't need to load the entire user to validate the security stamp.
        // just project the user into the security stamp.
        var userSecurityStamp = await dbContext.Users
            .Where(x => x.Id == userId.Value)
            .Select(x => x.SecurityStamp)
            .FirstOrDefaultAsync(ct);

        return authenticationState.User.GetSecurityStamp() == userSecurityStamp;
    }

    public void ForceSignOut()
    {
        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        var anonymousState = new AuthenticationState(anonymousUser);
        SetAuthenticationState(Task.FromResult(anonymousState));
    }
}