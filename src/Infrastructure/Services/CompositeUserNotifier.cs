using Application.Services;
using Domain.Aggregates;
using Domain.Entities;
using Domain.ValueObjects;

namespace Infrastructure.Services;

public sealed class CompositeUserNotifier(EmailUserNotifier emailNotifier, SmsUserNotifier smsNotifier) : IUserNotifier
{
    public async Task SendEmailConfirmationAsync(User user, string callback, CancellationToken ct = default)
    {
        await emailNotifier.SendEmailConfirmationAsync(user, callback, ct);
    }

    public async Task SendYourAccountHasBeenAddedAsync(User user, string password, CancellationToken ct = default)
    {
        await emailNotifier.SendYourAccountHasBeenAddedAsync(user, password, ct);
    }

    public async Task SendWelcomeAsync(User user, CancellationToken ct = default)
    {
        await emailNotifier.SendWelcomeAsync(user, ct);
    }

    public async Task SendNewLoginLocationAsync(User user, UserLogin login, CancellationToken ct = default)
    {
        if (user.NotifyBySms)
            await smsNotifier.SendNewLoginLocationAsync(user,login, ct);

        await emailNotifier.SendNewLoginLocationAsync(user,login, ct);
    }

    public async Task SendPasswordResetAsync(User user, string callback, CancellationToken ct = default)
    {
        await emailNotifier.SendPasswordResetAsync(user, callback, ct);
    }

    public async Task SendPasswordChangedAsync(User user, CancellationToken ct = default)
    {
        await emailNotifier.SendPasswordChangedAsync(user, ct);
    }

    public async Task SendDeleteAccountConfirmationAsync(User user, CancellationToken ct = default)
    {
        await emailNotifier.SendDeleteAccountConfirmationAsync(user, ct);
    }

    public async Task NotifyPackageArrivedAtWarehouse(User staff, Package package, CancellationToken ct = default)
    {
        if (package.Owner.NotifyBySms)
            await smsNotifier.NotifyPackageArrivedAtWarehouse(staff, package, ct);

        await emailNotifier.NotifyPackageArrivedAtWarehouse(staff, package, ct);
    }

    public async Task NotifyPackageSentToDestination(User staff, Package package, CancellationToken ct = default)
    {
        if (package.Owner.NotifyBySms)
            await smsNotifier.NotifyPackageSentToDestination(staff, package, ct);

        await emailNotifier.NotifyPackageSentToDestination(staff, package, ct);
    }

    public async Task NotifyPackageArrivedAtDestination(User staff, Package package, CancellationToken ct = default)
    {
        if (package.Owner.NotifyBySms)
            await smsNotifier.NotifyPackageArrivedAtDestination(staff, package, ct);

        await emailNotifier.NotifyPackageArrivedAtDestination(staff, package, ct);
    }

    public async Task NotifyPackageDelivered(Package package, CancellationToken ct = default)
    {
        if (package.Owner.NotifyBySms)
            await smsNotifier.NotifyPackageDelivered(package, ct);

        await emailNotifier.NotifyPackageDelivered(package, ct);
    }

    public async Task NotifyPackageIsDeemedProhibited(Package package, CancellationToken ct = default)
    {
        if (package.Owner.NotifyBySms)
            await smsNotifier.NotifyPackageIsDeemedProhibited(package, ct);

        await emailNotifier.NotifyPackageIsDeemedProhibited(package, ct);
    }

    public async Task NotifyTopUpSuccess(User user, Money amount, PaymentMethod paymentMethod, object paymentSession, CancellationToken ct = default)
    {
        if (user.NotifyBySms)
            await smsNotifier.NotifyTopUpSuccess(user, amount, paymentMethod, paymentSession, ct);

        await emailNotifier.NotifyTopUpSuccess(user, amount, paymentMethod, paymentSession, ct);
    }

    public async Task NotifyPaidForPackageSuccessfully(User user, Package package, CancellationToken ct = default)
    {
        if (user.NotifyBySms)
            await smsNotifier.NotifyPaidForPackageSuccessfully(user, package, ct);

        await emailNotifier.NotifyPaidForPackageSuccessfully(user, package, ct);
    }
}