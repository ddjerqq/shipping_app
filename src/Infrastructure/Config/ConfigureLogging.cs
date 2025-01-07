using System.Net.Security;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Application;
using Application.Common;
using Destructurama;
using Domain.Common;
using Generated;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Templates.Themes;
using SerilogTracing.Expressions;

namespace Infrastructure.Config;

public sealed class ConfigureLogging : ConfigurationBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .Configure()
            .CreateLogger();

        services.AddSerilog();
        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(Log.Logger, true));
    }
}

public static class LoggingExt
{
    public static LoggerConfiguration Configure(this LoggerConfiguration config)
    {
        SelfLog.Enable(Console.Error);

        var levelSwitch = new LoggingLevelSwitch();

        var seqHost = "SEQ__HOST".FromEnvRequired();
        var seqApiKey = "SEQ__API_KEY".FromEnvRequired();

        var messageHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = ValidateCloudflareOriginCertificate,
        };

        return config
            .MinimumLevel.ControlledBy(levelSwitch)
            .MinimumLevel.Override("Quartz", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Destructure.UsingAttributes()
            .Destructure.ByTransforming<Ulid>(id => id.ToString())
            .Destructure.ByTransforming<IStrongId>(id => id.ToString()!)
            .Enrich.WithProperty("Application", "SEQ__APP_NAME".FromEnvRequired())
            .Enrich.FromLogContext()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithAssemblyName()
            .WriteTo.Debug()
            .WriteTo.Seq(seqHost,
                apiKey: seqApiKey,
                messageHandler: messageHandler,
                period: TimeSpan.FromSeconds(10),
                controlLevelSwitch: levelSwitch)
            .WriteTo.Console(Formatters.CreateConsoleTextFormatter(TemplateTheme.Code));
    }

    private static bool ValidateCloudflareOriginCertificate(HttpRequestMessage _, X509Certificate2? cert, X509Chain? __, SslPolicyErrors ___) =>
        cert is not null
        // CN=CloudFlare Origin Certificate, OU=CloudFlare Origin CA, O="CloudFlare, Inc."
        && cert.Subject.Contains("CN=CloudFlare Origin Certificate")
        && cert.Subject.Contains("OU=CloudFlare Origin CA")
        && cert.Subject.Contains("O=\"CloudFlare, Inc.\"")
        // OU=CloudFlare Origin SSL ECC Certificate Authority, O="CloudFlare, Inc.", L=San Francisco, S=California, C=US
        && cert.Issuer.Contains("OU=CloudFlare Origin SSL ECC Certificate Authority")
        && cert.Issuer.Contains("O=\"CloudFlare, Inc.\"")
        && cert.Issuer.Contains("L=San Francisco")
        && cert.Issuer.Contains("S=California")
        && cert.Issuer.Contains("C=US");

    public static void UseConfiguredSerilogRequestLogging(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.Logger = Log.Logger;
            options.IncludeQueryInRequestPath = true;
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("UserId", httpContext.User.FindFirstValue(ClaimsPrincipalExt.IdClaimType) ?? "unauthenticated");
                diagnosticContext.Set("ClientAddress", httpContext.Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
                diagnosticContext.Set("ClientUserAgent", (string?)httpContext.Request.Headers.UserAgent);
                diagnosticContext.Set("TraceIdentifier", httpContext.TraceIdentifier);
            };
        });
    }

    public static void UseConfiguredSerilog(this ConfigureHostBuilder host)
    {
        host.UseSerilog((_, configuration) => configuration.Configure());
    }
}