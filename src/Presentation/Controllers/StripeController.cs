using Application.Services;
using Domain.Aggregates;
using Domain.Common;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace Presentation.Controllers;

[ApiController]
[AllowAnonymous]
public sealed class StripeController(ICurrentUserAccessor currentUserAccessor, StripePaymentService paymentService) : ControllerBase
{
    [HttpGet("topup")]
    public async Task<IActionResult> RequestTopUp([FromQuery] long amount, [FromQuery] string currency, CancellationToken ct = default)
    {
        var user = await currentUserAccessor.GetCurrentUserAsync(ct);
        var paymentUrl = await paymentService.CreatePaymentUrl(user, amount, currency, ct);
        return Redirect(paymentUrl);
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> HandleStripeWebhook(CancellationToken ct = default)
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(ct);
        var stripeSig = Request.Headers["Stripe-Signature"];
        var success = await paymentService.ValidatePaymentSuccessAsync(json, stripeSig!, ct);
        return success ? Ok() : BadRequest();
    }
}