using System.Numerics;
using Application.Services;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Cqrs.Packages.Commands;

public enum ReceivePackageAtWarehouseResult
{
    /// <summary>
    /// Package succesfully registered as received
    /// </summary>
    Success,

    /// <summary>
    /// Returned when the tracking code can't be found (in other words, the user has not pre-declared)
    /// </summary>
    NoOwnerFound,

    /// <summary>
    /// Returned when the package is already added
    /// </summary>
    PackageAlreadyInWarehouse,
}

public sealed record ReceivePackageAtWarehouseCommand : IRequest<ReceivePackageAtWarehouseResult>
{
    /// <summary>
    /// The tracking code of the package
    /// </summary>
    public string TrackingCode { get; set; } = null!;

    /// <summary>
    /// Address2 as present on the package label, i.e. the user room code
    /// </summary>
    public string? Address2 { get; set; }

    /// <summary>
    /// kilo grams
    /// </summary>
    public float WeightKiloGrams { get; set; }

    /// <summary>
    /// x centimeters
    /// </summary>
    public float Length { get; set; }

    /// <summary>
    /// y centimeters
    /// </summary>
    public float Width { get; set; }

    /// <summary>
    /// z centimeters
    /// </summary>
    public float Height { get; set; }
}

public sealed class ReceivePackageAtWarehouseValidator : AbstractValidator<ReceivePackageAtWarehouseCommand>
{
    public ReceivePackageAtWarehouseValidator()
    {
        RuleFor(x => x.TrackingCode).NotEmpty();
    }
}

internal sealed class ReceivePackageAtWarehouseCommandHandler(
    IAppDbContext dbContext,
    ILogger<ReceivePackageAtWarehouseCommandHandler> logger,
    ISender sender,
    ICurrentUserAccessor currentUser)
    : IRequestHandler<ReceivePackageAtWarehouseCommand, ReceivePackageAtWarehouseResult>
{
    public async Task<ReceivePackageAtWarehouseResult> Handle(ReceivePackageAtWarehouseCommand request, CancellationToken ct)
    {
        var package = await dbContext.Packages
            .Include(x => x.Owner)
            .FirstOrDefaultAsync(x => x.TrackingCode == request.TrackingCode, ct);

        var user = int.TryParse(request.Address2, out var roomCode)
            ? await dbContext.Users.FirstOrDefaultAsync(x => x.RoomCode == roomCode, ct)
            : package?.Owner;

        var receiver = await currentUser.GetCurrentUserAsync(ct);

        if (package is null)
        {
            if (user is null)
                logger.LogWarning("Orphan package received: {TrackingCode}. Will add the package to the receiver {ReceiverId}.", request.TrackingCode, receiver.Id);

            // create the package, and add it to the user.
            var createPackageCommand = new ReceiveUndeclaredPackageAtWarehouse(
                request.TrackingCode,
                user ?? receiver,
                receiver,
                request.Width,
                request.Height,
                request.Length,
                request.WeightKiloGrams,
                DateTime.UtcNow);
            await sender.Send(createPackageCommand, ct);

            return user is null
                ? ReceivePackageAtWarehouseResult.NoOwnerFound
                : ReceivePackageAtWarehouseResult.Success;
        }

        if (package.Statuses.Any(packageStatus => packageStatus.Status == PackageStatus.InWarehouse))
            return ReceivePackageAtWarehouseResult.PackageAlreadyInWarehouse;

        package.ArrivedAtWarehouse(receiver, new Vector3(request.Width, request.Height, request.Length), (long)(request.WeightKiloGrams * 1000), DateTime.UtcNow);

        await dbContext.SaveChangesAsync(ct);
        return ReceivePackageAtWarehouseResult.Success;
    }
}