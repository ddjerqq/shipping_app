using Application.Services;
using Domain.Events;
using MediatR;
using Serilog;

namespace Application.Cqrs.Packages.Events;

internal sealed class PackageArrivedAtDestinationEventHandler(
    IAppDbContext dbContext,
    IUserNotifier notifier)
    : INotificationHandler<PackageArrivedAtDestination>
{
    public async Task Handle(PackageArrivedAtDestination notification, CancellationToken ct)
    {
        var staff = await dbContext.Users.FindAsync([notification.StaffId], ct);
        var package = await dbContext.Packages.FindAsync([notification.PackageId], ct);

        Log.Information(
            "Package {PackageId} has arrived at destination. Received by {StaffId} at {Date}",
            notification.PackageId, staff?.Id, notification.Date);

        await notifier.NotifyPackageArrivedAtDestination(staff!, package!, ct);
    }
}