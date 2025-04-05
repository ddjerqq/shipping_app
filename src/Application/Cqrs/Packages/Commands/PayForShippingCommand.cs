using Application.Services;
using Domain.Aggregates;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Cqrs.Packages.Commands;

public sealed record PayForShippingCommand(PackageId PackageId) : IRequest;

public sealed class PayForShippingValidator : AbstractValidator<PayForShippingCommand>
{
    public PayForShippingValidator(IAppDbContext dbContext, ICurrentUserAccessor currentUserAccessor)
    {
        RuleSet("async", () =>
        {
            RuleFor(x => x.PackageId)
                .CustomAsync(async (id, ctx, ct) =>
                {
                    var package = await dbContext.Packages
                        .Include(x => x.Owner)
                        .FirstOrDefaultAsync(x => x.Id == id, ct);

                    var currentUser = await currentUserAccessor.GetCurrentUserAsync(ct);

                    if (package is null)
                    {
                        ctx.AddFailure("Package could not be found");
                        return;
                    }

                    if (package.IsPaid)
                    {
                        ctx.AddFailure("The package has already been paid for");
                        return;
                    }

                    if (package.CurrentStatus.Status != PackageStatus.Arrived)
                    {
                        ctx.AddFailure("You cannot pay for a package that has not arrived yet");
                        return;
                    }

                    if (package.OwnerId != currentUserAccessor.Id)
                    {
                        ctx.AddFailure("The current user does not own the specified package");
                        return;
                    }

                    if (package.Price!.TotalPrice > currentUser.Balance)
                    {
                        ctx.AddFailure("The current user does not have enough balance to pay for the package");
                        return;
                    }
                });
        });
    }
}

internal sealed class PayForShippingCommandHandler(IAppDbContext dbContext, ICurrentUserAccessor currentUserAccessor) : IRequestHandler<PayForShippingCommand>
{
    // TODO fix this
    public async Task Handle(PayForShippingCommand request, CancellationToken ct)
    {
        var package = await dbContext.Packages.FindAsync([request.PackageId], ct);
        var currentUser = await currentUserAccessor.GetCurrentUserAsync(ct);

        currentUser.PayForPackage(package!);

        await dbContext.SaveChangesAsync(ct);
    }
}