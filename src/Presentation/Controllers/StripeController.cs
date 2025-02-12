using Application.Services;
using Domain.Aggregates;
using Domain.Common;
using Domain.ValueObjects;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace Presentation.Controllers;

[ApiController]
[AllowAnonymous]
public sealed class StripeController(StripePaymentService paymentService) : ControllerBase
{
    [HttpPost("webhook")]
    public async Task<IActionResult> HandleStripeWebhook(CancellationToken ct = default)
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(ct);
        var stripeSig = Request.Headers["Stripe-Signature"];
        var success = await paymentService.ValidatePaymentSuccessAsync(json, stripeSig!, ct);
        return success ? Ok() : BadRequest();
    }
}