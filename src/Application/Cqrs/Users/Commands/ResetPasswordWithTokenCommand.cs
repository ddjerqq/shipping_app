using Application.Services;
using Destructurama.Attributed;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cqrs.Users.Commands;

public sealed record ResetPasswordWithTokenCommand : IRequest
{
    [NotLogged]
    public string NewPassword { get; set; } = null!;

    [NotLogged]
    public string ConfirmNewPassword { get; set; } = null!;

    public string Token { get; set; } = null!;
}

public sealed class ResetPasswordWithTokenValidator : AbstractValidator<ResetPasswordWithTokenCommand>
{
    public ResetPasswordWithTokenValidator(IJwtGenerator jwtGenerator)
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty().MinimumLength(12);

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().MinimumLength(12)
            .Equal(command => command.NewPassword).WithMessage("Passwords dont match!");

        RuleFor(x => x.Token)
            .NotEmpty()
            .MustAsync(async (token, _) => (await jwtGenerator.TryValidateTokenAsync(token)).Any())
            .WithMessage("Invalid token!");
    }
}

internal sealed class ResetPasswordWithTokenHandler(IAppDbContext dbContext, IUserVerificationTokenGenerator tokenGenerator) : IRequestHandler<ResetPasswordWithTokenCommand>
{
    public async Task Handle(ResetPasswordWithTokenCommand request, CancellationToken ct)
    {
        var result = await tokenGenerator.ValidateTokenAsync(IUserVerificationTokenGenerator.ResetPasswordPurpose, request.Token);
        if (result is null)
            return;

        var (_, securityStamp, userId) = result.Value;

        var user = await dbContext.Users.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == userId, ct);
        if (user is null)
            throw new InvalidOperationException($"User with id {userId} not found.");

        if (user.SecurityStamp != securityStamp)
            throw new InvalidOperationException("User auth credentials changed");

        user.SetPassword(request.NewPassword);
        await dbContext.SaveChangesAsync(ct);
    }
}