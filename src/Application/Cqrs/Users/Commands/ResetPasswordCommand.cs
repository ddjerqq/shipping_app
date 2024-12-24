using Application.Services;
using FluentValidation;
using MediatR;

namespace Application.Cqrs.Users.Commands;

public sealed record ResetPasswordCommand : IRequest
{
    public string NewPassword { get; set; } = default!;

    public string Token { get; set; } = default!;
}

public sealed class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator(IJwtGenerator jwtGenerator)
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(6).WithMessage("New password must be at least 6 characters.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required.")
            .Must(token => jwtGenerator.TryValidateToken(token, out _))
            .WithMessage("Invalid token!");
    }
}

internal sealed class ResetPasswordHandler(IAppDbContext dbContext, IUserVerificationTokenGenerator tokenGenerator) : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var result = tokenGenerator.ValidateToken(IUserVerificationTokenGenerator.ResetPasswordPurpose, request.Token);
        if (result is null)
            return;

        var (_, _, securityStamp, userId) = result.Value;

        var user = await dbContext.Users.FindAsync([userId], ct);
        if (user is null)
            throw new InvalidOperationException($"User with id {userId} not found.");

        if (user.SecurityStamp != securityStamp)
            throw new InvalidOperationException("User auth credentials changed");

        user.SetPassword(request.NewPassword);
        await dbContext.SaveChangesAsync(ct);
    }
}