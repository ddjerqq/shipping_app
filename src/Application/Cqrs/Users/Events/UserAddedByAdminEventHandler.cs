using Application.Services;
using Domain.Events;
using MediatR;

namespace Application.Cqrs.Users.Events;

internal sealed class UserAddedByAdminEventHandler(IAppDbContext dbContext, IUserNotifier notifier)
    : INotificationHandler<UserAddedByAdmin>
{
    public async Task Handle(UserAddedByAdmin notification, CancellationToken ct)
    {
        var user = await dbContext.Users.FindAsync([notification.UserId], ct)
                   ?? throw new InvalidOperationException($"Failed to load the user from the database, user with id: {notification.UserId} not found");

        await notifier.SendYourAccountHasBeenAddedAsync(user, notification.Password, ct);
    }
}