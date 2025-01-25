using Application.Services;
using Domain.Aggregates;
using Domain.Entities;

namespace Infrastructure.Services;

public sealed class EmailUserNotifier(IEmailMarkupProvider emailMarkupProvider, IEmailSender sender) : IUserNotifier
{
    public Task SendEmailConfirmationAsync(User user, string callback, CancellationToken ct = default)
    {
        var markup = emailMarkupProvider.GetEmailConfirmationMarkup(user, callback);
        return sender.SendAsync(user.Email, "Confirm your EMail", markup, ct);
    }

    public Task SendWelcomeAsync(User user, CancellationToken ct = default)
    {
        var markup = emailMarkupProvider.GetWelcomeEmailMarkup(user);
        return sender.SendAsync(user.Email, "Welcome", markup, ct);
    }

    public Task SendNewLoginLocationAsync(User user, UserLogin login, CancellationToken ct = default)
    {
        var markup = emailMarkupProvider.GetNewLoginLocationNotificationMarkup(user, login);
        return sender.SendAsync(user.Email, "New login location detected", markup, ct);
    }

    public Task SendPasswordResetAsync(User user, string callback, CancellationToken ct = default)
    {
        var markup = emailMarkupProvider.GetPasswordResetMarkup(user, callback);
        return sender.SendAsync(user.Email, "Reset your password", markup, ct);
    }

    public Task SendPasswordChangedAsync(User user, CancellationToken ct = default)
    {
        var markup = emailMarkupProvider.GetPasswordChangedNotificationMarkup(user);
        return sender.SendAsync(user.Email, "Your password was changed", markup, ct);
    }

    public Task SendDeleteAccountConfirmationAsync(User user, CancellationToken ct = default)
    {
        var markup = emailMarkupProvider.GetDeleteAccountConfirmationMarkup(user);
        return sender.SendAsync(user.Email, "Your account is scheduled for deletion", markup, ct);
    }

    public Task NotifyPackageArrivedAtWarehouse(User staff, Package package, CancellationToken ct = default)
    {
        var markup = emailMarkupProvider.GetPackageArrivedAtWarehouseMarkup(staff, package);
        return sender.SendAsync(staff.Email, "Your package has arrived at our warehouse!", markup, ct);
    }

    public Task NotifyPackageSentToDestination(User staff, Package package, CancellationToken ct = default)
    {
        var markup = emailMarkupProvider.GetPackageSentToDestinationMarkup(staff, package);
        return sender.SendAsync(staff.Email, "Your package is on it's way", markup, ct);
    }

    public Task NotifyPackageArrivedAtDestination(User staff, Package package, CancellationToken ct = default)
    {
        var markup = emailMarkupProvider.GetPackageArrivedAtDestinationMarkup(staff, package);
        return sender.SendAsync(staff.Email, "Your package has arrived", markup, ct);
    }

    public Task NotifyPackageDelivered(User staff, Package package, CancellationToken ct = default)
    {
        var markup = emailMarkupProvider.GetPackageDeliveredMarkup(staff, package);
        return sender.SendAsync(staff.Email, "Your package has been delivered", markup, ct);
    }
}