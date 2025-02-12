using Application.Services;
using BlazorTemplater;
using Domain.Aggregates;
using Domain.Entities;
using Domain.ValueObjects;
using Presentation.Components.EmailTemplates;

namespace Presentation.Services;

public sealed class RazorEmailMarkupProvider : IEmailMarkupProvider
{
    public string GetEmailConfirmationMarkup(User recipient, string callback) =>
        new ComponentRenderer<VerifyEmail>()
            .Set(c => c.User, recipient)
            .Set(c => c.CallbackUrl, callback)
            .Render();

    public string GetYourAccountHasBeenAddedByAdminMarkup(User user, string password) =>
        new ComponentRenderer<YourAccountHasBeenAddedByAnAdmin>()
            .Set(c => c.User, user)
            .Set(c => c.Password, password)
            .Render();

    public string GetWelcomeEmailMarkup(User user) =>
        new ComponentRenderer<WelcomeEmail>()
            .Render();

    public string GetNewLoginLocationNotificationMarkup(User user, UserLogin login) =>
        new ComponentRenderer<NewLoginEmail>()
            .Set(c => c.User, user)
            .Set(c => c.Login, login)
            .Render();

    public string GetPasswordResetMarkup(User user, string callback) =>
        new ComponentRenderer<ResetPasswordEmail>()
            .Set(c => c.User, user)
            .Set(c => c.CallbackUrl, callback)
            .Render();

    public string GetPasswordChangedNotificationMarkup(User user) =>
        new ComponentRenderer<PasswordChangedEmail>()
            .Set(c => c.User, user)
            .Render();

    public string GetDeleteAccountConfirmationMarkup(User user) =>
        new ComponentRenderer<DeleteAccountEmail>()
            .Set(c => c.User, user)
            .Render();

    public string GetPackageArrivedAtWarehouseMarkup(Package package) =>
        new ComponentRenderer<PackageStatusUpdatedEmail>()
            .Set(c => c.Status, PackageStatus.InWarehouse)
            .Set(c => c.Package, package)
            .Render();

    public string GetPackageSentToDestinationMarkup(Package package) =>
        new ComponentRenderer<PackageStatusUpdatedEmail>()
            .Set(c => c.Status, PackageStatus.InTransit)
            .Set(c => c.Package, package)
            .Render();

    public string GetPackageArrivedAtDestinationMarkup(Package package) =>
        new ComponentRenderer<PackageStatusUpdatedEmail>()
            .Set(c => c.Status, PackageStatus.Arrived)
            .Set(c => c.Package, package)
            .Render();

    public string GetPackageDeliveredMarkup(Package package) =>
        new ComponentRenderer<PackageStatusUpdatedEmail>()
            .Set(c => c.Status, PackageStatus.Delivered)
            .Set(c => c.Package, package)
            .Render();

    public string GetPackageIsDeemedProhibitedMarkup(Package package) =>
        new ComponentRenderer<PackageIsProhibitedEmail>()
            .Set(c => c.Package, package)
            .Render();

    public string GetTopUpSuccessMarkup(User user, Money amount, PaymentMethod paymentMethod, object paymentSession) =>
        new ComponentRenderer<TopUpSuccessEmail>()
            .Set(c => c.User, user)
            .Set(c => c.Amount, amount)
            .Set(c => c.PaymentMethod, paymentMethod)
            .Set(c => c.PaymentSession, paymentSession)
            .Render();

    public string GetPaidForPackageSuccessfully(User user, Package package) =>
        new ComponentRenderer<ShippingPaidSuccessfullyEmail>()
            .Set(c => c.User, user)
            .Set(c => c.Package, package)
            .Render();
}