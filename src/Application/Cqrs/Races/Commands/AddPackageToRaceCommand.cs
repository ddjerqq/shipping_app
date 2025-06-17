using Application.Services;
using Destructurama.Attributed;
using Domain.Aggregates;
using Domain.Entities;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Application.Cqrs.Races.Commands;

public sealed record AddPackageToRaceCommand : IRequest
{
    [LogMasked]
    public string RaceId { get; set; } = null!;

    [LogMasked]
    public string PackageId { get; set; } = null!;
}

public sealed class AddPackageToRaceValidator : AbstractValidator<AddPackageToRaceCommand>
{
    public AddPackageToRaceValidator(IAppDbContext dbContext)
    {
        RuleFor(x => x.RaceId)
            .NotEmpty()
            .Must(id => RaceId.TryParse(id, null, out _))
            .WithMessage("{PropertyName} is wrong format!");

        RuleFor(x => x.PackageId)
            .NotEmpty()
            .Must(id => PackageId.TryParse(id, null, out _))
            .WithMessage("{PropertyName} is wrong format!");

        RuleSet("async", () =>
        {
            RuleFor(x => x.RaceId)
                .MustAsync(async (_, idString, ct) =>
                {
                    var id = RaceId.Parse(idString);
                    var race = await dbContext.Races.FindAsync([id], ct);
                    return race!.Start > DateTime.UtcNow;
                })
                .WithMessage("Race take off must be in the future. You cannot add packages to old races");

            RuleFor(x => x.PackageId)
                .MustAsync(async (_, idString, ct) =>
                {
                    var id = PackageId.Parse(idString);
                    var package = await dbContext.Packages.FindAsync([id], ct);
                    return package!.CurrentStatus.Status == PackageStatus.InWarehouse;
                })
                .WithMessage("Package must be in warehouse. This package's status does not match!");
        });
    }
}

internal sealed class AddPackageToRaceCommandHandler(IAppDbContext dbContext, ICurrentUserAccessor currentUser) : IRequestHandler<AddPackageToRaceCommand>
{
    public async Task Handle(AddPackageToRaceCommand request, CancellationToken ct)
    {
        var race = await dbContext.Races.FindAsync([RaceId.Parse(request.RaceId)], ct);
        var package = await dbContext.Packages.FindAsync([PackageId.Parse(request.PackageId)], ct);

        if (race is null || package is null)
            throw new InvalidOperationException("Race or package not found");

        var staff = await currentUser.GetCurrentUserAsync(ct);

        package.SentToDestination(staff, race, DateTime.UtcNow);
        await dbContext.SaveChangesAsync(ct);
    }
}