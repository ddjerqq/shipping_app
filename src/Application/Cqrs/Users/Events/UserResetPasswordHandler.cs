using Application.Services;
using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Cqrs.Users.Events;

internal sealed class UserResetPasswordHandler(
    ILogger<UserResetPasswordHandler> logger,
    IAppDbContext dbContext,
    IUserNotifier notifier) : INotificationHandler<UserResetPassword>
{
    public async Task Handle(UserResetPassword notification, CancellationToken ct)
    {
        var user = await dbContext.Users.FindAsync([notification.UserId], ct)
                   ?? throw new InvalidOperationException($"Failed to load the user from the database, user with id: {notification.UserId} not found");

        logger.LogInformation("User {UserId} reset their password", user.Id);
        await notifier.SendPasswordChangedAsync(user,  ct);
    }
}