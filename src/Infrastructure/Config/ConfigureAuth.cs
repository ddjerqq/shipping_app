#pragma warning disable CS1591
using Application;
using Application.Services;
using Infrastructure.Services;
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

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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