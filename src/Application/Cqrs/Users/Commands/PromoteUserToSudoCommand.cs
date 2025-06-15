using Application.Common;
using Application.Services;
using Domain.Common;
using FluentValidation;
using MediatR;

namespace Application.Cqrs.Users.Commands;

public sealed record PromoteUserToSudoCommand(string Key) : IRequest<string>;

public sealed class PromoteUserToSudoCommandValidator : AbstractValidator<PromoteUserToSudoCommand>
{
    public PromoteUserToSudoCommandValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty()
            .Must(key => key == "SUDO__KEY".FromEnvRequired())
            .WithMessage("Invalid key.");
    }
}

public sealed class PromoteUserToSudoCommandHandler(IAppDbContext dbContext, IJwtGenerator jwtGenerator, ICurrentUserAccessor currentUser) : IRequestHandler<PromoteUserToSudoCommand, string>
{
    public async Task<string> Handle(PromoteUserToSudoCommand request, CancellationToken ct)
    {
        var user = await currentUser.TryGetCurrentUserAsync(ct);
        if (user is null)
            throw new UnauthorizedAccessException("You must be logged in to perform this action.");

        user.PromoteToSudo();
        await dbContext.SaveChangesAsync(ct);

        var token = jwtGenerator.GenerateToken(user);
        return token;
    }
}