using Domain.Aggregates;
using Domain.Entities;
using Microsoft.AspNetCore.Components;

namespace Application.Services;

public interface IEmailMarkupProvider
{
    public string GetEmailConfirmationMarkup(User recipient, string callback, CancellationToken ct = default);
    public string GetWelcomeEmailMarkup(User user, CancellationToken ct = default);
    public string GetNewLoginLocationNotificationMarkup(User user, UserLogin login, CancellationToken ct = default);
    public string GetPasswordResetMarkup(User user, string callback, CancellationToken ct = default);
    public string GetPasswordChangedNotificationMarkup(User user, CancellationToken ct = default);
}