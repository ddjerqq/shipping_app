namespace Domain.ValueObjects;

public readonly record struct WebAddress(string Value)
{
    public string Value { get; init; } = string.IsNullOrWhiteSpace(Value) || Value.Length > 255
        ? throw new FormatException("Currency code must be a 3-letter uppercase string")
        : Value;

    public static implicit operator string(WebAddress currency) => currency.Value;
    public static explicit operator WebAddress(string value) => new(value);
    public override string ToString() => Value;
}