using Application.Services;
using BlazorTemplater;
using Domain.Aggregates;
using Domain.Entities;
using Presentation.Components.EmailTemplates;

namespace Presentation.Services;

public sealed class RazorEmailMarkupProvider : IEmailMarkupProvider
{
    public string GetEmailConfirmationMarkup(User recipient, string callback) =>
        new ComponentRenderer<VerifyEmail>()
            .Set(c => c.User, recipient)
            .Set(c => c.CallbackUrl, callback)
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
}