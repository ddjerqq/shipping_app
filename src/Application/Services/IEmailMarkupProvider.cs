using Domain.Aggregates;
using Domain.Entities;

namespace Application.Services;

public interface IEmailMarkupProvider
{
    public string GetEmailConfirmationMarkup(User recipient, string callback);
    public string GetWelcomeEmailMarkup(User user);
    public string GetNewLoginLocationNotificationMarkup(User user, UserLogin login);
    public string GetPasswordResetMarkup(User user, string callback);
    public string GetPasswordChangedNotificationMarkup(User user);
    public string GetDeleteAccountConfirmationMarkup(User user);
}