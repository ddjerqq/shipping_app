using Application.Services;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Cqrs.Users.Events;

internal sealed class UserDeletedAccountEventHandler(
    ILogger<UserDeletedAccountEventHandler> logger,
    IAppDbContext dbContext,
    IUserNotifier notifier)
    : INotificationHandler<UserDeletedAccount>
{
    public async Task Handle(UserDeletedAccount notification, CancellationToken ct)
    {
        var user = await dbContext.Users.FindAsync([notification.UserId], ct)
                   ?? throw new InvalidOperationException($"Failed to load the user from the database, user with id: {notification.UserId} not found");

        logger.LogInformation("User {UserId} deleted their account", user.Id);
        await notifier.SendDeleteAccountConfirmationAsync(user, ct);
    }
}