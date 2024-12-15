using Domain.Aggregates;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Infrastructure.Services;

public sealed class IdentityNoOpEmailSender(GoogleMailSender emailSender) : IEmailSender<User>
{
    public Task SendConfirmationLinkAsync(User user, string email, string confirmationLink)
    {
        Console.WriteLine(confirmationLink);
        return Task.CompletedTask;
    }
    // emailSender.SendAsync("support@sangoway.com", email, "Confirm your email", $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

    public Task SendPasswordResetLinkAsync(User user, string email, string resetLink)
    {
        Console.WriteLine(resetLink);
        return Task.CompletedTask;
    }
    // emailSender.SendAsync("support@sangoway.com", email, "Reset your password", $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

    public Task SendPasswordResetCodeAsync(User user, string email, string resetCode)
    {
        Console.WriteLine(resetCode);
        return Task.CompletedTask;
    }
    // emailSender.SendAsync("support@sangoway.com", email, "Reset your password", $"Please reset your password using the following code: {resetCode}");
}