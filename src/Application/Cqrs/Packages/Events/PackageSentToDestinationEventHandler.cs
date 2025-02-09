using Application.Services;
using Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Cqrs.Packages.Events;

internal sealed class PackageSentToDestinationEventHandler(
    ILogger<PackageSentToDestinationEventHandler> logger,
    IAppDbContext dbContext,
    IUserNotifier notifier)
    : INotificationHandler<PackageSentToDestination>
{
    public async Task Handle(PackageSentToDestination notification, CancellationToken ct)
    {
        var staff = await dbContext.Users.FindAsync([notification.StaffId], ct);

        var package = await dbContext.Packages
            .Include(x => x.Owner)
            .FirstOrDefaultAsync(x => x.Id == notification.PackageId, ct);

        var race = await dbContext.Races.FindAsync([notification.RaceId], ct);

        logger.LogInformation(
            "Package {PackageId} has been added to the race {RaceId} by {StaffId} at {Date}",
            notification.PackageId, race?.Id, staff?.Id, notification.Date);

        await notifier.NotifyPackageSentToDestination(staff!, package!, ct);
    }
}