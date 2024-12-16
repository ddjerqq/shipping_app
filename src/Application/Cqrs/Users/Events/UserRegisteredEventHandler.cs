using System.Text;
using System.Text.Encodings.Web;
using Application.Services;
using Domain.Aggregates;
using Domain.Events;
using MediatR;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Application.Cqrs.Users.Events;

internal sealed class UserRegisteredEventHandler(
    ILogger<UserRegisteredEventHandler> logger,
    IDataProtectionProvider dataProtectionProvider,
    IAppDbContext dbContext,
    IEmailSender emailSender)
    : INotificationHandler<UserRegistered>
{
    private IDataProtector Protector => dataProtectionProvider.CreateProtector("email_confirmation");

    public async Task Handle(UserRegistered notification, CancellationToken ct)
    {
        var user = await dbContext.Users.FindAsync([notification.UserId], ct)
                   ?? throw new InvalidOperationException($"Failed to load the user from the database, user with id: {notification.UserId} not found");

        var code = GenerateToken(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = QueryHelpers.AddQueryString("account/confirmEmail",
            new Dictionary<string, string?> { ["userId"] = user.Id.ToString(), ["code"] = code });

        logger.LogInformation("User {UserId} registered, sending confirmation email", user.Id);

#if DEBUG
        logger.LogInformation("Confirmation link: {CallbackUrl}", callbackUrl);
#endif

        await emailSender.SendAsync("support@sangoway.com", user.Email, "email confirmation", HtmlEncoder.Default.Encode(callbackUrl), ct);
    }

    private string GenerateToken(User user)
    {
        string[] values =
        [
            "email_confirmation",
            DateTimeOffset.UtcNow.ToString("O"),
            user.Id.ToString(),
            user.SecurityStamp,
        ];
        var payload = string.Join(";", values);

        return Protector.Protect(payload);
    }
}