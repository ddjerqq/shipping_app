using Domain.ValueObjects;
using FluentValidation;

namespace Application.Cqrs.Packages.Commands;

public sealed class CalculatePriceCommand
{
    public float WeightKiloGrams { get; set; }

    public float Length { get; set; }

    public float Width { get; set; }

    public float Height { get; set; }

    public bool IsHouseDelivery { get; set; }

    public bool VolumetricApplied => PackagePrice.ShouldCalculateVolumetricWeight(Length, Width, Height);
    public Money VolumetricPrice => PackagePrice.GetVolumetricWeightPrice(Length, Width, Height);
    public Money Price => PackagePrice.GetTotalPrice(Length, Width, Height, (long)(WeightKiloGrams / 1000), IsHouseDelivery, new Money("USD", 800));
}

public sealed class CalculatePriceValidator : AbstractValidator<CalculatePriceCommand>
{
    public CalculatePriceValidator()
    {
        RuleFor(x => x.WeightKiloGrams).GreaterThan(0).LessThan(100);
        RuleFor(x => x.Length).GreaterThan(0).LessThan(1000);
        RuleFor(x => x.Width).GreaterThan(0).LessThan(1000);
        RuleFor(x => x.Height).GreaterThan(0).LessThan(1000);
    }
}