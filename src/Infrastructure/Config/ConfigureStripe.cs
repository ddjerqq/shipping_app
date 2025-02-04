using Application;
using Application.Services;
using Domain.Common;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace Infrastructure.Config;

public sealed class ConfigureStripe : ConfigurationBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        var stripeSKey = "STRIPE__SECRET_KEY".FromEnvRequired();
        StripeConfiguration.ApiKey = stripeSKey;

        services.AddScoped<StripePaymentService>();
        services.AddScoped<IPaymentService, StripePaymentService>();
        services.AddScoped<ICurrencyConverter, GeorgianNationalBankCurrencyConverter>();
    }
}
