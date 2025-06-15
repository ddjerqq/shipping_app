using System.Numerics;
using Application.Services;
using Destructurama.Attributed;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Application.Cqrs.Packages.Commands;

public enum ReceivePackageAtWarehouseResult
{
    /// <summary>
    /// Package successfully registered as received
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
    [LogMasked]
    public string TrackingCode { get; set; } = null!;

    /// <summary>
    /// Address2 as present on the package label, i.e. the user room code
    /// </summary>
    [LogMasked]
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

    public Money PricePerKg { get; set; } = new("USD", 800);
}

public sealed class ReceivePackageAtWarehouseValidator : AbstractValidator<ReceivePackageAtWarehouseCommand>
{
    public ReceivePackageAtWarehouseValidator()
    {
        RuleFor(x => x.TrackingCode).NotEmpty();
        RuleFor(x => x.PricePerKg)
            .Must(price => price.Amount > 0)
            .WithMessage("Amount must be a positive number")
            .Must(price => price.Amount % 10 == 0)
            .WithMessage("Amount must not contain single cents")
            .Must(price => price.Amount <= 1000)
            .WithMessage("Amount must be less than 10")
            .Must(price => price.Currency == "USD")
            .WithMessage("Currency must be USD");
    }
}

internal sealed class ReceivePackageAtWarehouseCommandHandler(
    IAppDbContext dbContext,
    ISender sender,
    ICurrentUserAccessor currentUser)
    : IRequestHandler<ReceivePackageAtWarehouseCommand, ReceivePackageAtWarehouseResult>
{
    public async Task<ReceivePackageAtWarehouseResult> Handle(ReceivePackageAtWarehouseCommand request, CancellationToken ct)
    {
        var package = await dbContext.Packages
            .Include(x => x.Owner)
            .FirstOrDefaultAsync(x => x.TrackingCode.Value == request.TrackingCode, ct);

        var user = int.TryParse(request.Address2, out var roomCode)
            ? await dbContext.Users.FirstOrDefaultAsync(x => x.RoomCode == roomCode, ct)
            : package?.Owner;

        var receiver = await currentUser.GetCurrentUserAsync(ct);

        if (package is null)
        {
            if (user is null)
                Log.Warning("Orphan package received at warehouse: {TrackingCode}. Will add the package to the receiver {ReceiverId}.", request.TrackingCode, receiver.Id);

            // create the package, and add it to the user.
            var createPackageCommand = new ReceiveUndeclaredPackageAtWarehouse(
                request.TrackingCode,
                user ?? receiver,
                receiver,
                request.Width,
                request.Height,
                request.Length,
                request.WeightKiloGrams,
                DateTime.UtcNow,
                request.PricePerKg);

            await sender.Send(createPackageCommand, ct);

            return user is null
                ? ReceivePackageAtWarehouseResult.NoOwnerFound
                : ReceivePackageAtWarehouseResult.Success;
        }

        if (package.Statuses.Any(packageStatus => packageStatus.Status == PackageStatus.InWarehouse))
            return ReceivePackageAtWarehouseResult.PackageAlreadyInWarehouse;

        package.ArrivedAtWarehouse(
            receiver,
            new Vector3(request.Width, request.Height, request.Length),
            (long)(request.WeightKiloGrams * 1000),
            DateTime.UtcNow,
            request.PricePerKg);

        await dbContext.SaveChangesAsync(ct);
        return ReceivePackageAtWarehouseResult.Success;
    }
}