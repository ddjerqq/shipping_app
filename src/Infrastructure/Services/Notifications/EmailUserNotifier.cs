using Application.Services;
using Domain.Aggregates;
using Domain.Entities;
using Domain.ValueObjects;

namespace Infrastructure.Services.Notifications;

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

    public async Task NotifyPackageArrivedAtWarehouse(User staff, Package package, CancellationToken ct = default)
    {
        var markup = emailMarkupProvider.GetPackageArrivedAtWarehouseMarkup(package);
        await sender.SendAsync(package.Owner.Email, "Your package has arrived at our warehouse!", markup, ct);

        if (package.Sender is not null)
            await sender.SendAsync(package.Sender.Email, "Your package has arrived at our warehouse!", markup, ct);
    }

    public async Task NotifyPackageSentToDestination(User staff, Package package, CancellationToken ct = default)
    {
        var markup = emailMarkupProvider.GetPackageSentToDestinationMarkup(package);
        await sender.SendAsync(package.Owner.Email, "Your package is on it's way", markup, ct);

        if (package.Sender is not null)
            await sender.SendAsync(package.Sender.Email, "Your package is on it's way", markup, ct);
    }

    public async Task NotifyPackageArrivedAtDestination(User staff, Package package, CancellationToken ct = default)
    {
        var markup = emailMarkupProvider.GetPackageArrivedAtDestinationMarkup(package);
        await sender.SendAsync(package.Owner.Email, "Your package has arrived - Please pay for shipping!", markup, ct);

        if (package.Sender is not null)
            await sender.SendAsync(package.Sender.Email, "Your package has arrived - Please pay for shipping!", markup, ct);
    }

    public async Task NotifyPackageDelivered(Package package, CancellationToken ct = default)
    {
        var markup = emailMarkupProvider.GetPackageDeliveredMarkup(package);
        await sender.SendAsync(package.Owner.Email, "Your package has been delivered", markup, ct);

        if (package.Sender is not null)
            await sender.SendAsync(package.Sender.Email, "Your package has been delivered", markup, ct);
    }

    public async Task NotifyPackageIsDeemedProhibited(Package package, CancellationToken ct = default)
    {
        var markup = emailMarkupProvider.GetPackageIsDeemedProhibitedMarkup(package);
        await sender.SendAsync(package.Owner.Email, "Your package has been deemed prohibited!", markup, ct);

        if (package.Sender is not null)
            await sender.SendAsync(package.Sender.Email, "Your package has been deemed prohibited!", markup, ct);
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