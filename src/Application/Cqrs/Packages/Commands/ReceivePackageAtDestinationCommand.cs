using Application.Services;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cqrs.Packages.Commands;

public sealed record ReceivePackageAtDestinationCommand : IRequest
{
    /// <summary>
    /// The tracking code of the package
    /// </summary>
    public string TrackingCode { get; set; } = null!;
}

public sealed class ReceivePackageAtDestinationValidator : AbstractValidator<ReceivePackageAtDestinationCommand>
{
    public ReceivePackageAtDestinationValidator()
    {
        RuleFor(x => x.TrackingCode).NotEmpty();
    }
}

internal sealed class ReceivePackageAtDestinationCommandHandler(
    IAppDbContext dbContext,
    ICurrentUserAccessor currentUser)
    : IRequestHandler<ReceivePackageAtDestinationCommand>
{
    public async Task Handle(ReceivePackageAtDestinationCommand request, CancellationToken ct)
    {
        var package = await dbContext.Packages
                          .Include(x => x.Owner)
                          .FirstOrDefaultAsync(x => x.TrackingCode.Value == request.TrackingCode, ct)
                      ?? throw new InvalidOperationException($"Package with tracking code: {request.TrackingCode} could not be found");

        var receiver = await currentUser.GetCurrentUserAsync(ct);

        if (package.Statuses.Any(packageStatus => packageStatus.Status == PackageStatus.Arrived))
            throw new InvalidOperationException($"Package with tracking code: {request.TrackingCode} has already arrived");

        package.ArrivedAtDestination(receiver, DateTime.UtcNow);
        await dbContext.SaveChangesAsync(ct);
    }
}