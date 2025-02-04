namespace Domain.ValueObjects;

/// <summary>
/// Helper class to calculate the package's price.
/// </summary>
/// <param name="Length">Centimeters</param>
/// <param name="Width">Centimeters</param>
/// <param name="Height">Centimeters</param>
/// <param name="Weight">Grams</param>
/// <param name="IsHouseDelivery"></param>
public sealed record PackagePrice(float Length, float Width, float Height, long Weight, bool IsHouseDelivery)
{
    /// <summary>
    /// Smallest unit of USD price per gram
    /// 800 cents per kilo gram.
    /// </summary>
    public const long PricePerKiloGram = 800;

    public const string DefaultCurrency = "USD";

    #region statics

    private static long RoundUp(long value) => value + (10 - value % 10) % 10;

    public static bool ShouldCalculateVolumetricWeight(float length, float width, float height) => length + width + height > 150;

    public static Money GetVolumetricWeightPrice(float length, float width, float height) => ShouldCalculateVolumetricWeight(length, width, height)
        ? new Money(DefaultCurrency, RoundUp((long)(length * width * height) / 6000))
        : new Money(DefaultCurrency, 0);

    public static Money GetWeightPrice(long weightGrams) => new(
        DefaultCurrency,
        RoundUp(weightGrams / 1000 * PricePerKiloGram));

    public static Money GetTotalPrice(float length, float width, float height, long weightGrams, bool isHouseDelivery) =>
        GetWeightPrice(weightGrams)
        + GetVolumetricWeightPrice(length, width, height)
        + new Money(DefaultCurrency, isHouseDelivery ? 500 : 0);

    #endregion

    public bool IsCalculatingVolumetricWeight => ShouldCalculateVolumetricWeight(Length, Width, Height);
    public Money VolumetricWeightPrice => GetVolumetricWeightPrice(Length, Width, Height);
    public Money WeightPrice => GetWeightPrice(Weight);
    public Money TotalPrice => GetTotalPrice(Length, Width, Height, Weight, IsHouseDelivery);
}