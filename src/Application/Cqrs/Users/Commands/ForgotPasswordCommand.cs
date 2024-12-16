using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Application.Services;
using Domain.Aggregates;
using EntityFrameworkCore.DataProtection.Extensions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace Application.Cqrs.Users.Commands;

public sealed record ForgotPasswordCommand : IRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";
}

internal sealed class ForgotPasswordCommandHandler(IAppDbContext dbContext) : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var user = await dbContext.Users.WherePdEquals(nameof(User.Email), request.Email.ToLowerInvariant()).FirstOrDefaultAsync(ct);
        if (user is null || !user.EmailConfirmed)
            // Don't reveal that the user does not exist or is not confirmed
            return;

        // var code = await UserManager.GeneratePasswordResetTokenAsync(user);
        // code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        // var callbackUrl = NavigationManager.GetUriWithQueryParameters(
        //     NavigationManager.ToAbsoluteUri("Account/ResetPassword").AbsoluteUri,
        //     new Dictionary<string, object?> { ["code"] = code });
        //
        // await EmailSender.SendPasswordResetLinkAsync(user, Input.Email, HtmlEncoder.Default.Encode(callbackUrl));
        //
        // RedirectManager.RedirectTo("Account/ForgotPasswordConfirmation");
    }
}