using Application.Services;
using Domain.Aggregates;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Application.Cqrs.Packages.Commands;

public sealed record CreatePackageCommand : IRequest<Package>
{
    public AbstractAddress Origin { get; init; } = default!;
    public AbstractAddress Destination { get; init; } = default!;

    public TrackingCode? TrackingCode { get; init; }
    public Category Category { get; init; } = default!;

    public string Description { get; init; } = default!;
    public WebAddress WebsiteAddress { get; init; } = default!;
    public Money RetailPrice { get; init; } = default!;
    public bool HouseDelivery { get; init; }

    public UserId OwnerId { get; init; } = default!;
    public User Owner { get; init; } = default!;
}

public sealed class CreatePackageValidator : AbstractValidator<CreatePackageCommand>
{
    public CreatePackageValidator()
    {
        RuleFor(x => x.Origin).NotEmpty();
        RuleFor(x => x.Destination).NotEmpty();
        RuleFor(x => x.TrackingCode).NotNull();
        RuleFor(x => x.Category).NotNull();
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.WebsiteAddress).NotNull();
        RuleFor(x => x.RetailPrice).NotNull();
        RuleFor(x => x.OwnerId).NotNull();
        RuleFor(x => x.Owner).NotNull();
    }
}

internal sealed class CreatePackageCommandHandler(IAppDbContext dbContext) : IRequestHandler<CreatePackageCommand, Package>
{
    public async Task<Package> Handle(CreatePackageCommand request, CancellationToken ct)
    {
        var package = Package.Create(
            request.Origin,
            request.Destination,
            request.TrackingCode,
            request.Category,
            request.Description,
            request.WebsiteAddress,
            request.RetailPrice,
            request.HouseDelivery,
            request.Owner);

        dbContext.Packages.Add(package);
        await dbContext.SaveChangesAsync(ct);
        return package;
    }
}