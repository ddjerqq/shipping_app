using Application.Services;
using Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Application.Cqrs.Users.Events;

internal sealed class UserLoggedInFromNewDeviceEventHandler(
    IAppDbContext dbContext,
    IUserNotifier notifier) : INotificationHandler<UserLoggedInFromNewDevice>
{
    public async Task Handle(UserLoggedInFromNewDevice notification, CancellationToken ct)
    {
        var user = await dbContext.Users
                       .FirstOrDefaultAsync(x => x.Id == notification.UserId, ct)
                   ?? throw new InvalidOperationException($"Failed to load the user from the database, user with id: {notification.UserId} not found");

        var login = user.Logins.FirstOrDefault(x => x.Id == notification.UserLoginId)
            ?? throw new InvalidOperationException($"Failed to load the user login from the database, login with id: {notification.UserLoginId} not found");

        Log.Information("User {UserId} logged in from a new location. {LoginId}", user.Id, notification.UserLoginId);
        await notifier.SendNewLoginLocationAsync(user, login, ct);
    }
}