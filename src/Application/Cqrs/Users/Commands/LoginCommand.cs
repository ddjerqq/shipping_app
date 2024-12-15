using Application.Common;
using Application.Services;
using Destructurama.Attributed;
using Domain.Aggregates;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Cqrs.Users.Commands;

public sealed record LoginCommand : IRequest<SignInResult>
{
    [LogMasked]
    public string Email { get; set; } = default!;

    [LogMasked]
    public string Password { get; set; } = default!;

    public bool RememberMe { get; set; }
}

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

internal sealed class LoginCommandHandler(SignInManager<User> signInManager) : IRequestHandler<LoginCommand, SignInResult>
{
    public async Task<SignInResult> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await signInManager.UserManager.FindByEmailAsync(request.Email);
        if (user == null)
            return SignInResult.Failed;

        return await signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, true);
    }
}