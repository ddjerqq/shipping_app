using System.Numerics;
using Application.Services;
using Domain.Aggregates;
using Domain.ValueObjects;
using MediatR;

namespace Application.Cqrs.Packages.Commands;

/// <summary>
/// Command to register a package that has not been pre-declared as received at the warehouse.
/// Creates a package in the system and adds it to the user,
/// also adds an event to the receiver that they received the package.
/// </summary>
internal sealed record ReceiveUndeclaredPackageAtWarehouse(
    string TrackingCode,
    User Owner,
    User Receiver,
    float Width,
    float Height,
    float Length,
    float WeightKiloGrams,
    DateTime ReceivedAt) : IRequest;

internal sealed class ReceiveUndeclaredPackageAtWarehouseHandler(IAppDbContext dbContext) : IRequestHandler<ReceiveUndeclaredPackageAtWarehouse>
{
    public async Task Handle(ReceiveUndeclaredPackageAtWarehouse request, CancellationToken ct)
    {
        var package = Package.Create(
            (TrackingCode)request.TrackingCode,
            Category.OtherConsumerProducts,
            "-",
            "-",
            new Money("USD", 1),
            1,
            false,
            request.Owner);

        package.ArrivedAtWarehouse(request.Receiver, new Vector3(request.Width, request.Height, request.Length), (long)(request.WeightKiloGrams * 1000), request.ReceivedAt);
        request.Owner.Packages.Add(package);

        await dbContext.SaveChangesAsync(ct);
    }
}