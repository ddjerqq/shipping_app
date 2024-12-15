using System.Text;
using System.Text.Encodings.Web;
using Application.Services;
using Destructurama.Attributed;
using Domain.Aggregates;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace Application.Cqrs.Users.Commands;

public sealed record RegisterCommand : IRequest<IdentityResult>
{
    [LogMasked]
    public string FullName { get; set; } = default!;

    [LogMasked]
    public string Email { get; set; } = default!;

    [LogMasked]
    public string PhoneNumber { get; set; } = default!;

    [LogMasked]
    public DateOnly BirthDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    [LogMasked]
    public string PersonalId { get; set; } = default!;

    [LogMasked]
    public string Password { get; set; } = default!;

    [LogMasked]
    public string Country { get; set; } = default!;

    [LogMasked]
    public string? State { get; set; } = default!;

    [LogMasked]
    public string City { get; set; } = default!;

    [LogMasked]
    public string ZipCode { get; set; } = default!;

    [LogMasked]
    public string Address { get; set; } = default!;
}

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FullName).MinimumLength(5).MaximumLength(32);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).MinimumLength(12).MaximumLength(256);
        RuleFor(x => x.PhoneNumber).MinimumLength(10).MaximumLength(15);
        RuleFor(x => x.BirthDate).LessThan(DateOnly.FromDateTime(DateTime.Now).AddYears(-18)).WithMessage("You must be 18 years old or older to use this service");
        RuleFor(x => x.PersonalId).Matches(@"(\d{11}|\d{3}\-\d{4}\-\d{3})").WithMessage("Must be 11 digit georgian ID or 3-4-3 digits american SSN");
        RuleFor(x => x.Country).Matches("(GEO|US)").WithMessage("Must be GEO or USA");
        RuleFor(x => x.State).NotEmpty();
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.ZipCode).Matches(@"^\d+").WithMessage("Must be digits only");
        RuleFor(x => x.Address).NotEmpty().MaximumLength(256);
    }
}

internal sealed class RegisterCommandHandler(UserManager<User> userManager, IUserStore<User> userStore, ILookupNormalizer lookupNormalizer, IEmailSender<User> emailSender, SignInManager<User> signInManager)
    : IRequestHandler<RegisterCommand, IdentityResult>
{
    public async Task<IdentityResult> Handle(RegisterCommand request, CancellationToken ct)
    {
        var user = new User
        {
            Id = UserId.New(),
            UserName = request.FullName,
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            PersonalId = request.PersonalId,
            AddressInfo = new FullAddress(request.Country, request.State ?? request.City, request.City, request.ZipCode, request.Address),
        };

        await userStore.SetUserNameAsync(user, request.FullName, ct);
        await userStore.SetNormalizedUserNameAsync(user, lookupNormalizer.NormalizeName(request.FullName), ct);

        await ((IUserEmailStore<User>)userStore).SetEmailAsync(user, request.Email, ct);
        await ((IUserEmailStore<User>)userStore).SetNormalizedEmailAsync(user, lookupNormalizer.NormalizeEmail(request.Email), ct);

        await ((IUserPhoneNumberStore<User>)userStore).SetPhoneNumberAsync(user, request.PhoneNumber, ct);
        await ((IUserPhoneNumberStore<User>)userStore).SetPhoneNumberConfirmedAsync(user, false, ct);

        var result = await userManager.CreateAsync(user, request.Password);

        var userId = await userManager.GetUserIdAsync(user);
        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = QueryHelpers.AddQueryString("account/confirmEmail",
            new Dictionary<string, string?> { ["userId"] = userId, ["code"] = code });

        await emailSender.SendConfirmationLinkAsync(user, request.Email, HtmlEncoder.Default.Encode(callbackUrl));

        if (!userManager.Options.SignIn.RequireConfirmedAccount)
            await signInManager.SignInAsync(user, isPersistent: false);

        return result;
    }
}