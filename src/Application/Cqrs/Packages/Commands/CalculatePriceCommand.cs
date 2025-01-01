using Domain.Common;
using Domain.ValueObjects;

namespace Application.Cqrs.Packages.Commands;

public sealed class CalculatePriceCommand
{
    public float WeightKiloGrams { get; set; }

    public float Length { get; set; }

    public float Width { get; set; }

    public float Height { get; set; }

    public bool VolumetricApplied => PackageExt.ShouldCalculateVolumetricWeight(Length, Width, Height);
    public Money VolumetricPrice => new("USD", PackageExt.GetVolumetricWeightPrice(Length, Width, Height));
    public Money Price => new("USD", PackageExt.GetTotalPrice(Length, Width, Height, WeightKiloGrams / 1000));
}