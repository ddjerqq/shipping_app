using Application.Services;
using Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cqrs.Packages.Events;

internal sealed class PackageArrivedAtWarehouseEventHandler(IAppDbContext dbContext) : INotificationHandler<PackageArrivedAtWarehouse>
{
    public async Task Handle(PackageArrivedAtWarehouse notification, CancellationToken ct)
    {
        var package = await dbContext.Packages.FindAsync([notification.PackageId], ct);
        var receiver = await dbContext.Users.FindAsync([notification.ReceiverId], ct);

        // notify the user that their package has arrived at the warehouse.
    }
}