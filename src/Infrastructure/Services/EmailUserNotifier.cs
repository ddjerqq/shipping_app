using Application.Services;
using Domain.Aggregates;
using Domain.Entities;

namespace Infrastructure.Services;

public sealed class EmailUserNotifier(IAuthEmailMarkupProvider authEmailMarkupProvider, IEmailSender sender) : IUserNotifier
{
    public Task SendEmailConfirmationAsync(User user, string callback, CancellationToken ct = default)
    {
        var markup = authEmailMarkupProvider.GetEmailConfirmationMarkup(user, callback);
        return sender.SendAsync(user.Email, "Confirm your EMail", markup, ct);
    }

    public Task SendWelcomeAsync(User user, CancellationToken ct = default)
    {
        var markup = authEmailMarkupProvider.GetWelcomeEmailMarkup(user);
        return sender.SendAsync(user.Email, "Welcome", markup, ct);
    }

    public Task SendNewLoginLocationAsync(User user, UserLogin login, CancellationToken ct = default)
    {
        var markup = authEmailMarkupProvider.GetNewLoginLocationNotificationMarkup(user, login);
        return sender.SendAsync(user.Email, "New login location detected", markup, ct);
    }

    public Task SendPasswordResetAsync(User user, string callback, CancellationToken ct = default)
    {
        var markup = authEmailMarkupProvider.GetPasswordResetMarkup(user, callback);
        return sender.SendAsync(user.Email, "Reset your password", markup, ct);
    }

    public Task SendPasswordChangedAsync(User user, CancellationToken ct = default)
    {
        var markup = authEmailMarkupProvider.GetPasswordChangedNotificationMarkup(user);
        return sender.SendAsync(user.Email, "Your password was changed", markup, ct);
    }

    public Task SendDeleteAccountConfirmationAsync(User user, CancellationToken ct = default)
    {
        var markup = authEmailMarkupProvider.GetDeleteAccountConfirmationMarkup(user);
        return sender.SendAsync(user.Email, "Your account is scheduled for deletion", markup, ct);
    }

    public Task NotifyPackageArrivedAtWarehouse(User user, Package package, CancellationToken ct = default)
    {
        var markup = authEmailMarkupProvider.GetPackageArrivedAtWarehouseMarkup(user, package);
        return sender.SendAsync(user.Email, "Your package has arrived at our warehouse!", markup, ct);
    }

    public Task NotifyPackageSentToDestination(User user, Package package, CancellationToken ct = default)
    {
        var markup = authEmailMarkupProvider.GetPackageSentToDestinationMarkup(user, package);
        return sender.SendAsync(user.Email, "Your package is on it's way", markup, ct);
    }

    public Task NotifyPackageArrivedAtDestination(User user, Package package, CancellationToken ct = default)
    {
        var markup = authEmailMarkupProvider.GetPackageArrivedAtDestinationMarkup(user, package);
        return sender.SendAsync(user.Email, "Your package has arrived", markup, ct);
    }

    public Task NotifyPackageDelivered(User user, Package package, CancellationToken ct = default)
    {
        var markup = authEmailMarkupProvider.GetPackageDeliveredMarkup(user, package);
        return sender.SendAsync(user.Email, "Your package has been delivered", markup, ct);
    }
}