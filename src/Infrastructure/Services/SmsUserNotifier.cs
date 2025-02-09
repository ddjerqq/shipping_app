using Application.Services;
using Domain.Aggregates;
using Domain.Entities;

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

    public Task NotifyPackageArrivedAtWarehouse(User staff, Package package, CancellationToken ct = default)
    {
        var content = $"Your package has arrived at our warehouse (tracking code: {package.TrackingCode})";
        return sender.SendAsync(package.Owner.PhoneNumber, content, ct);
    }

    public Task NotifyPackageSentToDestination(User staff, Package package, CancellationToken ct = default)
    {
        var content = $"Your package is on it's way (tracking code: {package.TrackingCode})";
        return sender.SendAsync(package.Owner.PhoneNumber, content, ct);
    }

    public Task NotifyPackageArrivedAtDestination(User staff, Package package, CancellationToken ct = default)
    {
        var content = $"Your package has arrived (tracking code: {package.TrackingCode}). Please pay for shipping as soon as possible!";
        return sender.SendAsync(package.Owner.PhoneNumber, content, ct);
    }

    public Task NotifyPackageDelivered(Package package, CancellationToken ct = default)
    {
        var content = $"Your package has been delivered (tracking code: {package.TrackingCode})";
        return sender.SendAsync(package.Owner.PhoneNumber, content, ct);
    }
}