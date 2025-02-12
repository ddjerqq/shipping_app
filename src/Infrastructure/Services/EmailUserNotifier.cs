using Application.Services;
using Domain.Aggregates;
using Domain.Entities;
using Domain.ValueObjects;

namespace Infrastructure.Services;

public sealed class EmailUserNotifier(IEmailMarkupProvider emailMarkupProvider, IEmailSender sender) : IUserNotifier
{
    public Task SendEmailConfirmationAsync(User user, string callback, CancellationToken ct = default)
    {
        var markup = emailMarkupProvider.GetEmailConfirmationMarkup(user, callback);
        return sender.SendAsync(user.Email, "Confirm your EMail", markup, ct);
    }

    public Task SendYourAccountHasBeenAddedAsync(User user, string password, CancellationToken ct = default)
    {
        var markup = emailMarkupProvider.GetYourAccountHasBeenAddedByAdminMarkup(user, password);
        return sender.SendAsync(user.Email, "Welcome", markup, ct);
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
        var markup = emailMarkupProvider.GetPackageArrivedAtWarehouseMarkup(package);
        return sender.SendAsync(package.Owner.Email, "Your package has arrived at our warehouse!", markup, ct);
    }

    public Task NotifyPackageSentToDestination(User staff, Package package, CancellationToken ct = default)
    {
        var markup = emailMarkupProvider.GetPackageSentToDestinationMarkup(package);
        return sender.SendAsync(package.Owner.Email, "Your package is on it's way", markup, ct);
    }

    public Task NotifyPackageArrivedAtDestination(User staff, Package package, CancellationToken ct = default)
    {
        var markup = emailMarkupProvider.GetPackageArrivedAtDestinationMarkup(package);
        return sender.SendAsync(package.Owner.Email, "Your package has arrived - Please pay for shipping!", markup, ct);
    }

    public Task NotifyPackageDelivered(Package package, CancellationToken ct = default)
    {
        var markup = emailMarkupProvider.GetPackageDeliveredMarkup(package);
        return sender.SendAsync(package.Owner.Email, "Your package has been delivered", markup, ct);
    }

    public Task NotifyTopUpSuccess(User user, Money amount, PaymentMethod paymentMethod, object paymentSession, CancellationToken ct = default)
    {
        var markup = emailMarkupProvider.GetTopUpSuccessMarkup(user, amount, paymentMethod, paymentSession);
        return sender.SendAsync(user.Email, "Your balance has been topped up!", markup, ct);
    }

    public Task NotifyPaidForPackageSuccessfully(User user, Package package, CancellationToken ct = default)
    {
        var markup = emailMarkupProvider.GetPaidForPackageSuccessfully(user, package);
        return sender.SendAsync(user.Email, "You successfully paid for shipping", markup, ct);
    }
}