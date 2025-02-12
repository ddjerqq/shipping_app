using Application.Services;
using Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cqrs.Users.Events;

public sealed class UserPaidForPackageEventHandler(IAppDbContext dbContext, IUserNotifier notifier) : INotificationHandler<UserPaidForPackage>
{
    public async Task Handle(UserPaidForPackage notification, CancellationToken ct)
    {
        var package = await dbContext.Packages
            .Include(x => x.Owner)
            .FirstOrDefaultAsync(x => x.Id == notification.PackageId, ct);

        await notifier.NotifyPaidForPackageSuccessfully(package!.Owner, package, ct);
    }
}