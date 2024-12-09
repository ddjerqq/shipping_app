namespace Domain.ValueObjects;

public record struct TrackingCode(string Value)
{
    public string Value { get; init; } =
        !string.IsNullOrWhiteSpace(Value)
        ? Value
        : throw new ArgumentException("Tracking code must not be empty", nameof(Value));

    public static implicit operator string(TrackingCode code) => code.Value;
    public static explicit operator TrackingCode(string code) => new(code);
    public override string ToString() => Value;

    public static TrackingCode New()
    {
        var a = Random.Shared.NextInt64(0, long.MaxValue);
        var b = Random.Shared.NextInt64(0, long.MaxValue);

        var code = $"{a}{b}"[..21];
        return new TrackingCode($"GE{code}TB");
    }
}