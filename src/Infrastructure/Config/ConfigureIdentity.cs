#pragma warning disable CS1591
using Application;
using Application.Common;
using Domain.Aggregates;
using Domain.Common;
using Domain.Entities;
using Infrastructure.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Persistence;

namespace Infrastructure.Config;

public sealed class ConfigureAuth : ConfigurationBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddCascadingAuthenticationState();
        services.AddScoped<IdentityUserAccessor>();
        services.AddScoped<IdentityRedirectManager>();
        services.AddScoped<IEmailSender<User>, IdentityNoOpEmailSender>();
        services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        services.AddIdentityCore<User>(options =>
            {
                options.ClaimsIdentity.UserIdClaimType = ClaimsPrincipalExt.IdClaimType;
                options.ClaimsIdentity.UserNameClaimType = ClaimsPrincipalExt.UsernameClaimType;
                options.ClaimsIdentity.RoleClaimType = ClaimsPrincipalExt.RoleClaimType;
                options.ClaimsIdentity.EmailClaimType = ClaimsPrincipalExt.EmailClaimType;
                options.ClaimsIdentity.SecurityStampClaimType = ClaimsPrincipalExt.SecurityStampClaimType;
                // claims identity
                options.ClaimsIdentity.UserIdClaimType = ClaimsPrincipalExt.IdClaimType;
                options.ClaimsIdentity.UserNameClaimType = ClaimsPrincipalExt.UsernameClaimType;
                options.ClaimsIdentity.RoleClaimType = ClaimsPrincipalExt.RoleClaimType;
                options.ClaimsIdentity.EmailClaimType = ClaimsPrincipalExt.EmailClaimType;
                options.ClaimsIdentity.SecurityStampClaimType = ClaimsPrincipalExt.SecurityStampClaimType;
                // options.Lockout;
                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredUniqueChars = 1;
                // change to true in prod
                // options.Tokens;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedAccount = false;
                // store
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyz ";
            })
            .AddSignInManager()
            .AddRoles<Role>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddUserStore<UserStore<User, Role, AppDbContext, UserId, IdentityUserClaim<UserId>, IdentityUserRole<UserId>, IdentityUserLogin<UserId>, IdentityUserToken<UserId>, IdentityRoleClaim<UserId>>>()
            .AddDefaultTokenProviders();

        services.AddAuthorizationBuilder()
            .AddDefaultPolicy("default", policy => policy.RequireAuthenticatedUser())
            .AddPolicy("is_elon", policy => policy.RequireClaim(ClaimsPrincipalExt.UsernameClaimType, "elon"));
    }
}