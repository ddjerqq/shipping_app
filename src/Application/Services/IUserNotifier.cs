using Domain.Aggregates;
using Domain.Entities;

namespace Application.Services;

public interface IUserNotifier
{
    #region auth

    /// <summary>
    /// Notifies a user that their email has been confirmed.
    /// </summary>
    public Task SendEmailConfirmationAsync(User user, string callback, CancellationToken ct = default);

    /// <summary>
    /// Notifies a user that their account has been added by an admin
    /// </summary>
    public Task SendYourAccountHasBeenAddedAsync(User user, string password, CancellationToken ct = default);

    /// <summary>
    /// Notifies a user that their account has been created. Sends a welcome message
    /// </summary>
    public Task SendWelcomeAsync(User user, CancellationToken ct = default);

    /// <summary>
    /// Notifies a user that a new login location has been detected to their account
    /// </summary>
    public Task SendNewLoginLocationAsync(User user, UserLogin login, CancellationToken ct = default);

    /// <summary>
    /// Notifies a user with a link to reset their password
    /// </summary>
    public Task SendPasswordResetAsync(User user, string callback, CancellationToken ct = default);

    /// <summary>
    /// Notifies a user that their password has been changed
    /// </summary>
    public Task SendPasswordChangedAsync(User user, CancellationToken ct = default);

    /// <summary>
    /// Notifies a user that their account is scheduled for deletion
    /// </summary>
    public Task SendDeleteAccountConfirmationAsync(User user, CancellationToken ct = default);

    #endregion

    #region package

    /// <summary>
    /// Notifies a user that their package has arrived at the warehouse
    /// </summary>
    public Task NotifyPackageArrivedAtWarehouse(User staff, Package package, CancellationToken ct = default);

    /// <summary>
    /// Notifies a user that their package has been sent to the destination
    /// </summary>
    public Task NotifyPackageSentToDestination(User staff, Package package, CancellationToken ct = default);

    /// <summary>
    /// Notifies a user that their package has arrived at the destination
    /// </summary>
    public Task NotifyPackageArrivedAtDestination(User staff, Package package, CancellationToken ct = default);

    /// <summary>
    /// Notifies a user that their package has been delivered
    /// </summary>
    public Task NotifyPackageDelivered(Package package, CancellationToken ct = default);

    #endregion
}