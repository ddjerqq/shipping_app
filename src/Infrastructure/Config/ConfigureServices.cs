using Application;
using Application.Services;
using Infrastructure.Services;
using Infrastructure.Services.Notifications;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Config;

public sealed class ConfigureInfrastructure : ConfigurationBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddDistributedMemoryCache();
        services.AddHttpContextAccessor();

        services.AddScoped<BrowserInternalizationProvider>();
        services.AddScoped<CookieService>();

        services.AddScoped<IRecaptchaVerifier, GoogleRecaptchaVerifier>();
        services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();
        services.AddScoped<IUserVerificationTokenGenerator, JwtUserVerificationTokenGenerator>();
        services.AddScoped<IIpGeoLocator, IpInfoIpGeoLocator>();

        services.AddScoped<IEmailSender, GoogleMailSender>();
        services.AddScoped<ISmsSender, SmsOfficeSmsSender>();

        services.AddScoped<SmsUserNotifier>();
        services.AddScoped<EmailUserNotifier>();
        services.AddScoped<IUserNotifier, CompositeUserNotifier>();
    }
}