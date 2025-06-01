#pragma warning disable CS1591
using System.Security.Claims;
using Application;
using Application.Services;
using Domain.Common;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Config;

public sealed class ConfigureAuth : ConfigurationBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddCascadingAuthenticationState();

        services.AddSingleton<IJwtGenerator, JwtGenerator>();
        services.AddScoped<ILookupNormalizer, LowerInvariantLookupNormalizer>();
        services.AddScoped<IdentityRevalidatingAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<IdentityRevalidatingAuthenticationStateProvider>());

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddGoogle("Google", options =>
            {
                options.ClientId = "GOOGLE__SSO_CLIENT_ID".FromEnvRequired();
                options.ClientSecret = "GOOGLE__SSO_CLIENT_SECRET".FromEnvRequired();
                options.SaveTokens = true;
                options.CallbackPath = "/signin-google";

                options.Scope.Add("openid");
                options.Scope.Add("email");
                options.Scope.Add("profile");

                options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/auth/login";
                options.AccessDeniedPath = "/auth/denied";
            })
            .AddJwtBearer(options =>
            {
                var serviceProvider = services.BuildServiceProvider();
                var jwtGenerator = (serviceProvider.GetRequiredService<IJwtGenerator>() as JwtGenerator)!;
                
                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.Audience = JwtGenerator.ClaimsAudience;
                options.ClaimsIssuer = JwtGenerator.ClaimsIssuer;
                
                options.Events = Events;
                options.TokenValidationParameters = jwtGenerator.TokenValidationParameters;
            });

        // services.AddAuthorizationBuilder()
        //     .AddDefaultPolicy("default", policy => policy.RequireAuthenticatedUser())
        //     .AddPolicy("is_elon", policy => policy.RequireClaim(ClaimsPrincipalExt.UsernameClaimType, "elon"));
    }
    
    private static readonly JwtBearerEvents Events = new()
    {
        OnMessageReceived = ctx =>
        {
            ctx.Request.Query.TryGetValue(JwtGenerator.CookieName, out var query);
            ctx.Request.Headers.TryGetValue(JwtGenerator.CookieName, out var header);
            ctx.Request.Cookies.TryGetValue(JwtGenerator.CookieName, out var cookie);
            ctx.Token = (string?)query ?? (string?)header ?? cookie;
            return Task.CompletedTask;
        },
        
        OnForbidden = ctx =>
        {
            if (ctx.Request.Path.StartsWithSegments("/api"))
            {
                ctx.Response.StatusCode = 403;
                return Task.CompletedTask;
            }

            ctx.Response.Redirect("auth/denied");
            return Task.CompletedTask;
        },
        
        OnChallenge = ctx =>
        {
            if (ctx.Request.Path.StartsWithSegments("/api"))
            {
                ctx.Response.StatusCode = 401;
                return Task.CompletedTask;
            }

            ctx.Response.Redirect("auth/login");
            ctx.HandleResponse();
            return Task.CompletedTask;
        },
    };
}