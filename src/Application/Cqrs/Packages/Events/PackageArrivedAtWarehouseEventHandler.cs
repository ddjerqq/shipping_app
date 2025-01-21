using Application.Services;
using Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cqrs.Packages.Events;

internal sealed class PackageArrivedAtWarehouseEventHandler(IAppDbContext dbContext, IUserNotifier notifier) : INotificationHandler<PackageArrivedAtWarehouse>
{
    public async Task Handle(PackageArrivedAtWarehouse notification, CancellationToken ct)
    {
        var package = await dbContext.Packages.FindAsync([notification.PackageId], ct);
        var user = await dbContext.Users.FindAsync([notification.OwnerId], ct);
        await notifier.NotifyPackageArrivedAtWarehouse(user!, package!, ct);
    }
}