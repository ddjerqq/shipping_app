using Domain.Aggregates;
using Domain.Entities;
using Domain.ValueObjects;

namespace Application.Services;

public interface IEmailMarkupProvider
{
    public string GetEmailConfirmationMarkup(User user, string callback);
    public string GetYourAccountHasBeenAddedByAdminMarkup(User user, string password);
    public string GetWelcomeEmailMarkup(User user);
    public string GetNewLoginLocationNotificationMarkup(User user, UserLogin login);
    public string GetPasswordResetMarkup(User user, string callback);
    public string GetPasswordChangedNotificationMarkup(User user);
    public string GetDeleteAccountConfirmationMarkup(User user);

    public string GetPackageArrivedAtWarehouseMarkup(Package package);
    public string GetPackageSentToDestinationMarkup(Package package);
    public string GetPackageArrivedAtDestinationMarkup(Package package);
    public string GetPackageDeliveredMarkup(Package package);
    public string GetPackageIsDeemedProhibitedMarkup(Package package);

    public string GetTopUpSuccessMarkup(User user, Money amount, PaymentMethod paymentMethod, object paymentSession);
    public string GetPaidForPackageSuccessfully(User user, Package package);
}