using Domain.Aggregates;
using Domain.Entities;
using Domain.ValueObjects;

namespace Application.Services;

public interface IEmailMarkupProvider
{
    public string GetEmailConfirmationMarkup(User user, string callback);
    public string GetWelcomeEmailMarkup(User user);
    public string GetNewLoginLocationNotificationMarkup(User user, UserLogin login);
    public string GetPasswordResetMarkup(User user, string callback);
    public string GetPasswordChangedNotificationMarkup(User user);
    public string GetDeleteAccountConfirmationMarkup(User user);

    public string GetPackageArrivedAtWarehouseMarkup(User user, Package package);
    public string GetPackageSentToDestinationMarkup(User user, Package package);
    public string GetPackageArrivedAtDestinationMarkup(User user, Package package);
    public string GetPackageDeliveredMarkup(User user, Package package);
}