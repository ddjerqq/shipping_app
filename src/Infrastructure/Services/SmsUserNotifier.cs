using Application.Services;
using Domain.Aggregates;
using Domain.Entities;
using Domain.ValueObjects;

namespace Infrastructure.Services;

public sealed class SmsUserNotifier(ISmsSender sender) : IUserNotifier
{
    public Task SendEmailConfirmationAsync(User user, string callback, CancellationToken ct = default)
    {
        throw new NotImplementedException("Sms user notifier does not support the following method");
    }

    public Task SendYourAccountHasBeenAddedAsync(User user, string password, CancellationToken ct = default)
    {
        throw new NotImplementedException("Sms user notifier does not support the following method");
    }

    public Task SendWelcomeAsync(User user, CancellationToken ct = default)
    {
        throw new NotImplementedException("Sms user notifier does not support the following method");
    }

    public Task SendNewLoginLocationAsync(User user, UserLogin login, CancellationToken ct = default)
    {
        var content = $"New login location detected from {login.Location}";
        return sender.SendAsync(user.PhoneNumber, content, ct);
    }

    public Task SendPasswordResetAsync(User user, string callback, CancellationToken ct = default)
    {
        throw new NotImplementedException("Sms user notifier does not support the following method");
    }

    public Task SendPasswordChangedAsync(User user, CancellationToken ct = default)
    {
        var content = "Your password was changed";
        return sender.SendAsync(user.PhoneNumber, content, ct);
    }

    public Task SendDeleteAccountConfirmationAsync(User user, CancellationToken ct = default)
    {
        var content = "Your account is scheduled for deletion";
        return sender.SendAsync(user.PhoneNumber, content, ct);
    }

    public async Task NotifyPackageArrivedAtWarehouse(User staff, Package package, CancellationToken ct = default)
    {
        var content = $"Your package has arrived at our warehouse (tracking code: {package.TrackingCode})";
        await sender.SendAsync(package.Owner.PhoneNumber, content, ct);

        if (package.Sender is not null)
            await sender.SendAsync(package.Sender.PhoneNumber, content, ct);
    }

    public async Task NotifyPackageSentToDestination(User staff, Package package, CancellationToken ct = default)
    {
        var content = $"Your package is on it's way (tracking code: {package.TrackingCode})";
        await sender.SendAsync(package.Owner.PhoneNumber, content, ct);

        if (package.Sender is not null)
            await sender.SendAsync(package.Sender.PhoneNumber, content, ct);
    }

    public async Task NotifyPackageArrivedAtDestination(User staff, Package package, CancellationToken ct = default)
    {
        var content = $"Your package has arrived (tracking code: {package.TrackingCode}). Please pay for shipping as soon as possible!";
        await sender.SendAsync(package.Owner.PhoneNumber, content, ct);

        if (package.Sender is not null)
            await sender.SendAsync(package.Sender.PhoneNumber, content, ct);
    }

    public async Task NotifyPackageDelivered(Package package, CancellationToken ct = default)
    {
        var content = $"Your package has been delivered (tracking code: {package.TrackingCode})";
        await sender.SendAsync(package.Owner.PhoneNumber, content, ct);

        if (package.Sender is not null)
            await sender.SendAsync(package.Sender.PhoneNumber, content, ct);
    }

    public async Task NotifyPackageIsDeemedProhibited(Package package, CancellationToken ct = default)
    {
        var content = $"Your package has been deemed prohibited (tracking code: {package.TrackingCode}). \n" +
                      $"Please contact our support team for more information! support@sangoway.com \n" +
                      $"For a full list of prohibited items please see: https://www.rs.ge/OnlineOrders?cat=3&tab=1";
        await sender.SendAsync(package.Owner.PhoneNumber, content, ct);

        if (package.Sender is not null)
            await sender.SendAsync(package.Sender.PhoneNumber, content, ct);
    }

    public async Task NotifyTopUpSuccess(User user, Money amount, PaymentMethod paymentMethod, object paymentSession, CancellationToken ct = default)
    {
        var content = $"Your balance has been topped up by {amount.FormatedValue}. Payment method: {paymentMethod}. Thank you for using our services!";
        await sender.SendAsync(user.PhoneNumber, content, ct);
    }

    public async Task NotifyPaidForPackageSuccessfully(User user, Package package, CancellationToken ct = default)
    {
        var content = $"You have successfully paid the shipping prices for package {package.TrackingCode}. Thank you for using our services!";
        await sender.SendAsync(user.PhoneNumber, content, ct);
    }
}