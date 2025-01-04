using Application.Services;
using Domain.Aggregates;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;

namespace Application.Cqrs.Packages.Commands;

public sealed record CreatePackageCommand : IRequest<Package>
{
    public string TrackingCode { get; set; } = null!;
    public Category Category { get; set; } = Category.OtherConsumerProducts;

    public string Description { get; set; } = null!;
    public string WebsiteAddress { get; set; } = null!;
    public Money RetailPrice { get; set; } = new("USD", 0);
    public int ItemCount { get; set; } = 1;
    public bool HouseDelivery { get; set; }

    public IBrowserFile? PictureFile { get; set; }
    public IBrowserFile? InvoiceFile { get; set; }
}

public sealed class CreatePackageValidator : AbstractValidator<CreatePackageCommand>
{
    public CreatePackageValidator()
    {
        RuleFor(x => x.TrackingCode).NotNull().MinimumLength(10).MaximumLength(64).Must(TrackingCode.IsValid).WithMessage("Tracking code is malformed!");
        RuleFor(x => x.Category).NotNull();
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.WebsiteAddress).NotNull();
        RuleFor(x => x.RetailPrice).NotNull().Must(x => x.Amount >= 0).WithMessage("Amount should be positive");
        RuleFor(x => x.ItemCount).NotNull().GreaterThanOrEqualTo(1);

        RuleFor(x => x.InvoiceFile)
            .Must(x => x!.Size <= 1_000_000).WithMessage("The file is too big")
            .Must(x => Path.GetExtension(x!.Name) is ".pdf").WithMessage("The file must be a .pdf format")
            .When(command => command.InvoiceFile is not null);

        RuleFor(x => x.PictureFile)
            .Must(x => x!.Size <= 1_000_000).WithMessage("The file is too big")
            .Must(x => Path.GetExtension(x!.Name) is ".jpg" or ".jpeg" or ".png").WithMessage("The file must be a .jpg, .jpeg, or .png format")
            .When(command => command.PictureFile is not null);
    }
}

internal sealed class CreatePackageCommandHandler(IAppDbContext dbContext, ICurrentUserAccessor currentUser, IFileStore fileStore) : IRequestHandler<CreatePackageCommand, Package>
{
    public async Task<Package> Handle(CreatePackageCommand request, CancellationToken ct)
    {
        var owner = await currentUser.GetCurrentUserAsync(ct);

        var package = Package.Create(
            (TrackingCode)request.TrackingCode,
            request.Category,
            request.Description,
            request.WebsiteAddress,
            request.RetailPrice,
            request.ItemCount,
            request.HouseDelivery,
            owner);

        if (request.InvoiceFile is not null)
        {
            await using var fileStream = request.InvoiceFile.OpenReadStream(cancellationToken: ct);
            package.InvoiceFileKey = await fileStore.CreateFileAsync(fileStream, Path.GetExtension(request.InvoiceFile.Name), ct);
        }

        if (request.PictureFile is not null)
        {
            await using var fileStream = request.PictureFile.OpenReadStream(cancellationToken: ct);
            package.PictureFileKey = await fileStore.CreateFileAsync(fileStream, Path.GetExtension(request.PictureFile.Name), ct);
        }

        dbContext.Packages.Add(package);
        await dbContext.SaveChangesAsync(ct);
        return package;
    }
}