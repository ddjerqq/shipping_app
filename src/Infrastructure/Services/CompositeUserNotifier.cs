using Application.Services;
using Domain.Aggregates;
using Domain.Entities;

namespace Infrastructure.Services;

public sealed class CompositeUserNotifier(EmailUserNotifier emailNotifier, SmsUserNotifier smsNotifier) : IUserNotifier
{
    public async Task SendEmailConfirmationAsync(User user, string callback, CancellationToken ct = default)
    {
        if (user.NotifyBySms)
            await smsNotifier.SendEmailConfirmationAsync(user, callback, ct);

        await emailNotifier.SendEmailConfirmationAsync(user, callback, ct);
    }

    public async Task SendWelcomeAsync(User user, CancellationToken ct = default)
    {
        if (user.NotifyBySms)
            await smsNotifier.SendWelcomeAsync(user, ct);

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
        if (user.NotifyBySms)
            await smsNotifier.SendPasswordResetAsync(user, callback, ct);

        await emailNotifier.SendPasswordResetAsync(user, callback, ct);
    }

    public async Task SendPasswordChangedAsync(User user, CancellationToken ct = default)
    {
        if (user.NotifyBySms)
            await smsNotifier.SendPasswordChangedAsync(user, ct);

        await emailNotifier.SendPasswordChangedAsync(user, ct);
    }

    public async Task SendDeleteAccountConfirmationAsync(User user, CancellationToken ct = default)
    {
        if (user.NotifyBySms)
            await smsNotifier.SendDeleteAccountConfirmationAsync(user, ct);

        await emailNotifier.SendDeleteAccountConfirmationAsync(user, ct);
    }

    public async Task NotifyPackageArrivedAtWarehouse(User user, Package package, CancellationToken ct = default)
    {
        if (user.NotifyBySms)
            await smsNotifier.NotifyPackageArrivedAtWarehouse(user, package, ct);

        await emailNotifier.NotifyPackageArrivedAtWarehouse(user, package, ct);
    }

    public async Task NotifyPackageSentToDestination(User user, Package package, CancellationToken ct = default)
    {
        if (user.NotifyBySms)
            await smsNotifier.NotifyPackageSentToDestination(user, package, ct);

        await emailNotifier.NotifyPackageSentToDestination(user, package, ct);
    }

    public async Task NotifyPackageArrivedAtDestination(User user, Package package, CancellationToken ct = default)
    {
        if (user.NotifyBySms)
            await smsNotifier.NotifyPackageArrivedAtDestination(user, package, ct);

        await emailNotifier.NotifyPackageArrivedAtDestination(user, package, ct);
    }

    public async Task NotifyPackageDelivered(User user, Package package, CancellationToken ct = default)
    {
        if (user.NotifyBySms)
            await smsNotifier.NotifyPackageDelivered(user, package, ct);

        await emailNotifier.NotifyPackageDelivered(user, package, ct);
    }
}