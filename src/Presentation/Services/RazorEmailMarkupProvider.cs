using Application.Services;
using BlazorTemplater;
using Domain.Aggregates;
using Domain.Entities;
using Presentation.Components.EmailTemplates;

namespace Presentation.Services;

public sealed class RazorEmailMarkupProvider : IEmailMarkupProvider
{
    public string GetEmailConfirmationMarkup(User recipient, string callback, CancellationToken ct = default) =>
        new ComponentRenderer<VerifyEmail>()
            .Set(c => c.User, recipient)
            .Set(c => c.CallbackUrl, callback)
            .Render();

    public string GetWelcomeEmailMarkup(User user, CancellationToken ct = default) =>
        new ComponentRenderer<WelcomeEmail>()
            .Render();

    public string GetNewLoginLocationNotificationMarkup(User user, UserLogin login, CancellationToken ct = default) =>
        new ComponentRenderer<NewLoginEmail>()
            .Set(c => c.User, user)
            .Set(c => c.Login, login)
            .Render();

    public string GetPasswordResetMarkup(User user, string callback, CancellationToken ct = default) =>
        new ComponentRenderer<ResetPasswordEmail>()
            .Set(c => c.User, user)
            .Set(c => c.CallbackUrl, callback)
            .Render();

    public string GetPasswordChangedNotificationMarkup(User user, CancellationToken ct = default) =>
        new ComponentRenderer<PasswordChangedEmail>()
            .Set(c => c.User, user)
            .Render();
}