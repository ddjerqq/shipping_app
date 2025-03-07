using Application.Services;
using Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Cqrs.Packages.Events;

internal sealed class PackageDeliveredEventHandler(
    ILogger<PackageDeliveredEventHandler> logger,
    IAppDbContext dbContext,
    IUserNotifier notifier)
    : INotificationHandler<PackageDelivered>
{
    public async Task Handle(PackageDelivered notification, CancellationToken ct)
    {
        var package = await dbContext.Packages.FindAsync([notification.PackageId], ct);

        logger.LogInformation(
            "Package {PackageId} has been delivered to the user at {Date}",
            notification.PackageId, notification.Date);

        await notifier.NotifyPackageDelivered(package!, ct);
    }
}