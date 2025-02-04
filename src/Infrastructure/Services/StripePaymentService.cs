using Application.Services;
using Domain.Aggregates;
using Domain.Common;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace Infrastructure.Services;

/// <summary>
/// This service handles payments using the stripe integration.
/// </summary>
/// <remarks>
/// example cards <br/>
/// 4242424242424242 success <br/>
/// 4000000000003220 authentication required<br/>
/// 4000000000000002 declined<br/>
/// </remarks>
public sealed class StripePaymentService(IAppDbContext dbContext, ICurrencyConverter currencyConverter, ILogger<StripePaymentService> logger) : IPaymentService
{
    private static string WebAppDomain => "WEB_APP__DOMAIN".FromEnvRequired();
    private static string WebhookSecret => "STRIPE__WEBHOOK_SECRET".FromEnvRequired();

    private readonly SessionService _sessionService = new();

    public async Task<string> CreatePaymentUrl(User user, long amountInCents, Currency currency, CancellationToken ct = default)
    {
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = ["card"],
            Mode = "payment",
            CustomerEmail = user.Email,
            SuccessUrl = $"https://{WebAppDomain}/payments/success?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = $"https://{WebAppDomain}/payments/cancel",
            LineItems =
            [
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = currency.Value.ToLower(),
                        UnitAmount = amountInCents,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Balance top up",
                            Description = "Top up your balance for Sangoway.com to pay for packages",
                        },
                    },
                    Quantity = 1,
                },
            ],
            Metadata = new Dictionary<string, string>
            {
                { nameof(UserId), user.Id.ToString() },
            },
        };

        var session = await _sessionService.CreateAsync(options, null, ct);
        return session.Url;
    }

    public async Task<bool> ValidatePaymentSuccessAsync(string json, string stripeSignature, CancellationToken ct = default)
    {
        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, WebhookSecret);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to construct stripe event");
            return false;
        }

        if (stripeEvent.Type is "checkout.session.completed")
        {
            var session = (stripeEvent.Data.Object as Session)!;
            var amountCharged = new Money(session.Currency.ToUpper(), session.AmountTotal ?? 0);

            if (session.Metadata.TryGetValue(nameof(UserId), out var userIdString) && UserId.TryParse(userIdString, null, out var userId))
            {
                var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId, ct);
                if (user is null)
                    throw new InvalidOperationException("User not found");

                // TODO add a list of stripe session ids to track how much they top up and spend and so on.
                var convertedAmount = await currencyConverter.ConvertToAsync(amountCharged, user.Balance.Currency, ct);
                user.AddBalance(convertedAmount);
                await dbContext.SaveChangesAsync(ct);

                logger.LogInformation("User {UserId} balance topped up by {Amount}. Stripe Session id: {StripeSessionId}", user.Id, convertedAmount, session.Id);
            }

            return true;
        }

        logger.LogInformation("Received stripe event: {EventType}: {EventId}", stripeEvent.Type, stripeEvent.Id);

        return true;
    }
}