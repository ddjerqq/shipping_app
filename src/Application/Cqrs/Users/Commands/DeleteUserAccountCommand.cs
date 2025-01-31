using Application.Services;
using Domain.Aggregates;
using Domain.Events;
using MediatR;

namespace Application.Cqrs.Users.Commands;

public sealed record DeleteUserAccountCommand(UserId UserId) : IRequest;

internal sealed class DeleteUserAccountCommandHandler(IAppDbContext dbContext) : IRequestHandler<DeleteUserAccountCommand>
{
    public async Task Handle(DeleteUserAccountCommand request, CancellationToken ct)
    {
        var user = await dbContext.Users.FindAsync([request.UserId], ct);
        if (user is null)
            throw new InvalidOperationException("User not found");

        user.AddDomainEvent(new UserDeletedAccount(user.Id));
        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync(ct);
    }
}