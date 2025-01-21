using Application.Services;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Cqrs.Users.Events;

internal sealed class UserConfirmedEmailEventHandler(
    ILogger<UserConfirmedEmailEventHandler> logger,
    IAppDbContext dbContext,
    IUserNotifier notifier)
    : INotificationHandler<UserConfirmedEmail>
{
    public async Task Handle(UserConfirmedEmail notification, CancellationToken ct)
    {
        var user = await dbContext.Users.FindAsync([notification.UserId], ct)
                   ?? throw new InvalidOperationException($"Failed to load the user from the database, user with id: {notification.UserId} not found");

        logger.LogInformation("User {UserId} confirmed email", user.Id);
        await notifier.SendWelcomeAsync(user, ct);
    }
}