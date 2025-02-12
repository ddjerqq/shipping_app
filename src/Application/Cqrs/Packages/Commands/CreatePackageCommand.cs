using Application.Services;
using Domain.Aggregates;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace Application.Cqrs.Packages.Commands;

public sealed record CreatePackageCommand : IRequest<Package>
{
    public const long MaxFileSizeBytes = 1_000_000;

    public User Owner { get; set; } = null!;

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
    public CreatePackageValidator(IAppDbContext dbContext)
    {
        RuleFor(x => x.TrackingCode).NotNull().MinimumLength(10).MaximumLength(64).Must(TrackingCode.IsValid).WithMessage("Tracking code is malformed!");
        RuleFor(x => x.Category).NotNull();
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.WebsiteAddress).NotNull();
        RuleFor(x => x.RetailPrice).NotNull().Must(x => x.Amount >= 0).WithMessage("Amount should be positive");
        RuleFor(x => x.ItemCount).NotNull().GreaterThanOrEqualTo(1);

        RuleFor(x => x.InvoiceFile)
            .Must(x => x!.Size <= CreatePackageCommand.MaxFileSizeBytes).WithMessage("The file is too big, must be less than 1mb")
            .Must(x => Path.GetExtension(x!.Name) is ".pdf").WithMessage("The file must be a .pdf format")
            .When(command => command.InvoiceFile is not null);

        RuleFor(x => x.PictureFile)
            .Must(x => x!.Size <= CreatePackageCommand.MaxFileSizeBytes).WithMessage("The file is too big, must be less than 1mb")
            .Must(x => Path.GetExtension(x!.Name) is ".jpg" or ".jpeg" or ".png").WithMessage("The file must be a .jpg, .jpeg, or .png format")
            .When(command => command.PictureFile is not null);

        RuleFor(x => x.HouseDelivery)
            .Must(houseDelivery => houseDelivery == false)
            .When(command => command.Owner is { AddressInfo: NoAddress } or null)
            .WithMessage(
                "You cannot choose house delivery right now, because you do not have a house address added. " +
                "Please add it by going to your account settings > my address");

        RuleSet("async", () =>
        {
            RuleFor(x => x.TrackingCode)
                .MustAsync(async (_, code, ct) =>
                {
                    var packagesWithCode = await dbContext.Packages.CountAsync(x => x.TrackingCode.Value == code, ct);
                    return packagesWithCode == 0;
                })
                .WithMessage("A package with that tracking code already exists");
        });
    }
}

internal sealed class CreatePackageCommandHandler(IAppDbContext dbContext, ICurrentUserAccessor currentUser, IFileStore fileStore) : IRequestHandler<CreatePackageCommand, Package>
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

        if (request.InvoiceFile is not null)
        {
            await using var fileStream = request.InvoiceFile.OpenReadStream(CreatePackageCommand.MaxFileSizeBytes, ct);
            package.InvoiceFileKey = await fileStore.CreateFileAsync(fileStream, Path.GetExtension(request.InvoiceFile.Name), ct);
        }

        if (request.PictureFile is not null)
        {
            await using var fileStream = request.PictureFile.OpenReadStream(CreatePackageCommand.MaxFileSizeBytes, ct);
            package.PictureFileKey = await fileStore.CreateFileAsync(fileStream, Path.GetExtension(request.PictureFile.Name), ct);
        }

        dbContext.Packages.Add(package);
        await dbContext.SaveChangesAsync(ct);
        return package;
    }
}