#pragma warning disable ASP0014
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Presentation.Common;
using Serilog;

namespace Presentation.Config;

/// <summary>
///     Web application extensions
/// </summary>
public static class WebAppExt
{
    /// <summary>
    ///     Apply any pending migrations to the database if any.
    /// </summary>
    public static async Task MigrateDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        await using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
        {
            var migrations = await dbContext.Database.GetPendingMigrationsAsync();
            Log.Information("Applying migrations: {Migrations}", string.Join(", ", migrations));
            await dbContext.Database.MigrateAsync();
        }

        Log.Information("All migrations applied");

        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    ///     Use general web app middleware
    /// </summary>
    public static void UseApplicationMiddleware(this WebApplication app)
    {
        app.UseGlobalExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        if (app.Environment.IsProduction())
        {
            // compress and then cache static files only in production
            app.UseResponseCompression();
            app.UseResponseCaching();
        }

        app.UseStaticFiles();

        app.UseRouting();
        app.UseRateLimiter();
        app.UseRequestLocalization(GetLocalizationOptions(app.Configuration));

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();
        app.UseCustomHeaderMiddleware();

        app.MapAppHealthChecks();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.MapControllers();

        app.MapFallback(ctx =>
        {
            // catch requests that dont really exist?
            ctx.Response.Redirect("/404");
            return Task.CompletedTask;
        });
    }

    private static void UseCustomHeaderMiddleware(this WebApplication app)
    {
        app.Use((ctx, next) =>
        {
            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Frame-Options
            ctx.Response.Headers.Append("X-Frame-Options", "DENY");

            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/X-Content-Type-Options
            ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");

            ctx.Response.Headers.Append("X-Made-By", "tenxdeveloper <g@nachkebia.dev>");

            return next();
        });
    }

    private static void UseGlobalExceptionHandler(this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }

    private static void MapAppHealthChecks(this IEndpointRouteBuilder app)
    {
        app.MapHealthChecks("/api/v1/health", new HealthCheckOptions
        {
            // if the predicate is null, then all checks are included
            // Predicate = _ => true,
            AllowCachingResponses = false,
        });
    }

    // TODO custom middleware here, if the user is authenticated, and the browser sends and Accept-Language header, then set and update the user's culture info accordingly.
    //     Accept-Language: <language>
    //     Accept-Language: *
    //     // Multiple types, weighted with the quality value syntax:
    //     Accept-Language: fr-CH, fr;q=0.9, en;q=0.8, de;q=0.7, *;q=0.5

    private static RequestLocalizationOptions GetLocalizationOptions(IConfiguration configuration)
    {
        var cultures = configuration.GetSupportedCultures().Keys.ToArray();
        return new RequestLocalizationOptions()
            .SetDefaultCulture(cultures[0])
            .AddSupportedCultures(cultures)
            .AddSupportedUICultures(cultures);
    }
}