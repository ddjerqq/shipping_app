using Application.Services;
using Domain.Aggregates;
using Domain.Entities;

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
        if (staff.NotifyBySms)
            await smsNotifier.NotifyPackageArrivedAtWarehouse(staff, package, ct);

        await emailNotifier.NotifyPackageArrivedAtWarehouse(staff, package, ct);
    }

    public async Task NotifyPackageSentToDestination(User staff, Package package, CancellationToken ct = default)
    {
        if (staff.NotifyBySms)
            await smsNotifier.NotifyPackageSentToDestination(staff, package, ct);

        await emailNotifier.NotifyPackageSentToDestination(staff, package, ct);
    }

    public async Task NotifyPackageArrivedAtDestination(User staff, Package package, CancellationToken ct = default)
    {
        if (staff.NotifyBySms)
            await smsNotifier.NotifyPackageArrivedAtDestination(staff, package, ct);

        await emailNotifier.NotifyPackageArrivedAtDestination(staff, package, ct);
    }

    public async Task NotifyPackageDelivered(User staff, Package package, CancellationToken ct = default)
    {
        if (staff.NotifyBySms)
            await smsNotifier.NotifyPackageDelivered(staff, package, ct);

        await emailNotifier.NotifyPackageDelivered(staff, package, ct);
    }
}