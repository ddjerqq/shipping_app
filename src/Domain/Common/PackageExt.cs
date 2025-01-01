using System.Net;
using System.Numerics;
using Domain.Aggregates;

namespace Domain.Common;

public static class PackageExt
{
    private static decimal RoundUp(decimal value) => Math.Ceiling(value * 100) / 100;

    public static bool ShouldCalculateVolumetricWeight(float x, float y, float z) => x + y + z > 150;
    public static bool ShouldCalculateVolumetricWeight(this Package package) => package.Dimensions is { X: var x, Y: var y, Z: var z } && ShouldCalculateVolumetricWeight(x, y, z);

    public static decimal GetVolumetricWeightPrice(float x, float y, float z) =>
        RoundUp(ShouldCalculateVolumetricWeight(x, y, z) ? (decimal)(x * y * z) / 6000 : 0);

    public static decimal GetVolumetricWeightPrice(this Package package) =>
        package.Dimensions is { X: var x, Y: var y, Z: var z } ? GetVolumetricWeightPrice(x, y, z) : 0;

    public static decimal GetWeightPrice(float weight) => RoundUp((decimal)weight / 1000 * Package.PricePerKg);
    public static decimal GetWeightPrice(this Package package) => GetWeightPrice(package.WeightGrams ?? 0);

    public static decimal GetTotalPrice(float x, float y, float z, float weight) =>
        GetWeightPrice(weight) + GetVolumetricWeightPrice(x, y, z);

    public static decimal GetTotalPrice(this Package package) =>
        package is { Dimensions: { X: var x, Y: var y, Z: var z }, WeightGrams: { } weightGrams }
            ? GetTotalPrice(x, y, z, weightGrams)
            : 0;
}