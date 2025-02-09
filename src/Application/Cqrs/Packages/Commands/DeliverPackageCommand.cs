using System.Numerics;
using Application.Services;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Cqrs.Packages.Commands;

public sealed record DeliverPackageCommand : IRequest
{
    /// <summary>
    /// The tracking code of the package
    /// </summary>
    public string TrackingCode { get; set; } = null!;
}

public sealed class DeliverPackageCommandValidator : AbstractValidator<DeliverPackageCommand>
{
    public DeliverPackageCommandValidator(IAppDbContext dbContext)
    {
        RuleFor(x => x.TrackingCode).NotEmpty();

        RuleSet("async", () =>
        {
            RuleFor(x => x.TrackingCode)
                .MustAsync(async (_, code, ct) =>
                {
                    var package = await dbContext.Packages.FirstOrDefaultAsync(p => p.TrackingCode.Value == code, ct);

                    if (package is null)
                        throw new InvalidOperationException($"Package with tracking code: {code} could not be found");

                    return package.IsPaid;
                })
                .WithMessage("Package must be paid for before it can be delivered");
        });
    }
}

internal sealed class DeliverPackageCommandCommandHandler(IAppDbContext dbContext) : IRequestHandler<DeliverPackageCommand>
{
    public async Task Handle(DeliverPackageCommand request, CancellationToken ct)
    {
        var package = await dbContext.Packages
                          .Include(x => x.Owner)
                          .FirstOrDefaultAsync(x => x.TrackingCode.Value == request.TrackingCode, ct)
                      ?? throw new InvalidOperationException($"Package with tracking code: {request.TrackingCode} could not be found");

        if (package.Statuses.Any(packageStatus => packageStatus.Status == PackageStatus.Delivered))
            throw new InvalidOperationException($"Package with tracking code: {request.TrackingCode} has already been delivered");

        package.Delivered(DateTime.UtcNow);
        await dbContext.SaveChangesAsync(ct);
    }
}