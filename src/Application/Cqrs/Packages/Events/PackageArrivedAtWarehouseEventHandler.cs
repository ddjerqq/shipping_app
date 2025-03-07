using Application.Services;
using Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Cqrs.Packages.Events;

internal sealed class PackageArrivedAtWarehouseEventHandler(
    ILogger<PackageArrivedAtWarehouseEventHandler> logger,
    IAppDbContext dbContext,
    IUserNotifier notifier)
    : INotificationHandler<PackageArrivedAtWarehouse>
{
    public async Task Handle(PackageArrivedAtWarehouse notification, CancellationToken ct)
    {
        var staff = await dbContext.Users.FindAsync([notification.StaffId], ct);
        var package = await dbContext.Packages.FindAsync([notification.PackageId], ct);

        logger.LogInformation(
            "Package {PackageId} has been received at the warehouse by {StaffId} at {Date}",
            notification.PackageId, staff?.Id, notification.Date);

        await notifier.NotifyPackageArrivedAtWarehouse(package!.Owner, package, ct);
    }
}