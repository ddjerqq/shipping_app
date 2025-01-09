using Domain.Aggregates;
using Domain.Entities;

namespace Application.Services;

public interface IEmailSender
{
    /// <summary>
    /// The email markup provider.
    /// </summary>
    protected IEmailMarkupProvider EmailMarkupProvider { get; }

    /// <summary>
    ///     Sends an email with the specified subject, content, recipients, and from address.
    /// </summary>
    protected Task SendAsync(string recipient, string subject, string content, CancellationToken ct = default);

    /// <summary>
    /// Sends an Email confirmation link to the specified user
    /// </summary>
    public Task SendEmailConfirmationAsync(User recipient, string callback, CancellationToken ct = default)
    {
        var markup = EmailMarkupProvider.GetEmailConfirmationMarkup(recipient, callback);
        return SendAsync(recipient.Email, "Confirm your email", markup, ct);
    }

    /// <summary>
    /// Sends a welcome email to the specified user
    /// </summary>
    public Task SendWelcomeEmailAsync(User user, CancellationToken ct = default)
    {
        var markup = EmailMarkupProvider.GetWelcomeEmailMarkup(user);
        return SendAsync(user.Email, "Welcome!", markup, ct);
    }

    /// <summary>
    /// Sends a notification to the user about a new login from an unknown location
    /// </summary>
    public Task SendNewLoginLocationNotificationAsync(User user, UserLogin login, CancellationToken ct = default)
    {
        var markup = EmailMarkupProvider.GetNewLoginLocationNotificationMarkup(user, login);
        return SendAsync(user.Email, "New login location detected", markup, ct);
    }

    /// <summary>
    /// Sends a password reset link to the specified user
    /// </summary>
    public Task SendPasswordResetAsync(User user, string callback, CancellationToken ct = default)
    {
        var markup = EmailMarkupProvider.GetPasswordResetMarkup(user, callback);
        return SendAsync(user.Email, "Reset your password", markup, ct);
    }

    /// <summary>
    /// Sends a notification to the user about a password change
    /// </summary>
    public Task SendPasswordChangedAsync(User user, CancellationToken ct = default)
    {
        var markup = EmailMarkupProvider.GetPasswordChangedNotificationMarkup(user);
        return SendAsync(user.Email, "Password changed", markup, ct);
    }

    /// <summary>
    /// Sends a notification to the user about their account being deleted
    /// </summary>
    public Task SendDeleteAccountConfirmation(User user, CancellationToken ct = default)
    {
        var markup = EmailMarkupProvider.GetDeleteAccountConfirmationMarkup(user);
        return SendAsync(user.Email, "Account deleted!", markup, ct);
    }
}