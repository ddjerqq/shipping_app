using Application.Services;
using Domain.Aggregates;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Application.Cqrs.Packages.Commands;

public sealed record CreatePackageCommand : IRequest<Package>
{
    public string TrackingCode { get; set; } = null!;
    public Category Category { get; set; } = Category.OtherConsumerProducts;

    public string Description { get; set; } = null!;
    public string WebsiteAddress { get; set; } = null!;
    public Money RetailPrice { get; set; } = default!;
    public int ItemCount { get; set; } = 1;
    public bool HouseDelivery { get; set; }

    public string? PictureKey { get; set; }
    public string? InvoiceKey { get; set; }

    public UserId OwnerId { get; set; } = default!;
    public User Owner { get; set; } = null!;
}

public sealed class CreatePackageValidator : AbstractValidator<CreatePackageCommand>
{
    public CreatePackageValidator()
    {
        RuleFor(x => x.TrackingCode).NotNull();
        RuleFor(x => x.Category).NotNull();
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.WebsiteAddress).NotNull();
        RuleFor(x => x.RetailPrice).NotNull();
        RuleFor(x => x.ItemCount).NotNull().GreaterThanOrEqualTo(1);
    }
}

internal sealed class CreatePackageCommandHandler(IAppDbContext dbContext) : IRequestHandler<CreatePackageCommand, Package>
{
    public async Task<Package> Handle(CreatePackageCommand request, CancellationToken ct)
    {
        var package = Package.Create(
            (TrackingCode)request.TrackingCode,
            request.Category,
            request.Description,
            request.WebsiteAddress,
            request.RetailPrice,
            request.ItemCount,
            request.HouseDelivery,
            request.Owner);

        dbContext.Packages.Add(package);
        await dbContext.SaveChangesAsync(ct);
        return package;
    }
}