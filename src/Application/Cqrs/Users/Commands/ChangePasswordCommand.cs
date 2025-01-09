using Application.Services;
using Destructurama.Attributed;
using FluentValidation;
using MediatR;

namespace Application.Cqrs.Users.Commands;

public sealed record ChangePasswordCommand : IRequest
{
    [LogMasked]
    public string CurrentPassword { get; set; } = null!;

    [LogMasked]
    public string NewPassword { get; set; } = null!;

    [LogMasked]
    public string ConfirmNewPassword { get; set; } = null!;
}

public sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator(ICurrentUserAccessor currentUser)
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().MinimumLength(12)
            .MustAsync(async (_, currentPassword, ct) =>
            {
                var user = await currentUser.GetCurrentUserAsync(ct);
                return BC.EnhancedVerify(currentPassword, user.PasswordHash);
            })
            .WithMessage("Invalid password!");

        RuleFor(x => x.NewPassword)
            .NotEmpty().MinimumLength(12);

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().MinimumLength(12)
            .Equal(command => command.NewPassword).WithMessage("Passwords dont match!");
    }
}

internal sealed class ChangePasswordHandler(ICurrentUserAccessor currentUser, IAppDbContext dbContext) : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var user = await currentUser.GetCurrentUserAsync(ct);
        if (user is null)
            throw new InvalidOperationException($"User with id {currentUser.Id} not found.");

        user.SetPassword(request.NewPassword);
        await dbContext.SaveChangesAsync(ct);
    }
}