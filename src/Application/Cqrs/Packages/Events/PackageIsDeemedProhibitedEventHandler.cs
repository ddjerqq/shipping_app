using Application.Services;
using Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cqrs.Packages.Events;

public sealed class PackageIsDeemedProhibitedEventHandler(IAppDbContext dbContext, IUserNotifier notifier) : INotificationHandler<PackageIsDeemedProhibited>
{
    public async Task Handle(PackageIsDeemedProhibited notification, CancellationToken ct)
    {
        var package = await dbContext.Packages.FindAsync([notification.PackageId], ct);
        await notifier.NotifyPackageIsDeemedProhibited(package!, ct);
    }
}