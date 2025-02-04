using Domain.ValueObjects;

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
    public Money Price => PackagePrice.GetTotalPrice(Length, Width, Height, (long)(WeightKiloGrams / 1000), IsHouseDelivery);
}