using Domain.Abstractions;
using Domain.Aggregates;
using Domain.Entities;
using Domain.ValueObjects;

namespace Application.Services;

public interface IUserNotifier
{
    #region auth

    public Task SendEmailConfirmationAsync(User user, string callback, CancellationToken ct = default);
    public Task SendWelcomeAsync(User user, CancellationToken ct = default);
    public Task SendNewLoginLocationAsync(User user, UserLogin login, CancellationToken ct = default);
    public Task SendPasswordResetAsync(User user, string callback, CancellationToken ct = default);
    public Task SendPasswordChangedAsync(User user, CancellationToken ct = default);
    public Task SendDeleteAccountConfirmationAsync(User user, CancellationToken ct = default);

    #endregion

    #region package

    public Task NotifyPackageArrivedAtWarehouse(User user, Package package, CancellationToken ct = default);
    public Task NotifyPackageSentToDestination(User user, Package package, CancellationToken ct = default);
    public Task NotifyPackageArrivedAtDestination(User user, Package package, CancellationToken ct = default);
    public Task NotifyPackageDelivered(User user, Package package, CancellationToken ct = default);

    #endregion
}