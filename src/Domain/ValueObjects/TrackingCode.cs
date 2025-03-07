using System.Text.RegularExpressions;

namespace Domain.ValueObjects;

public partial record struct TrackingCode(string Value)
{
    public string Value { get; init; } =
        !string.IsNullOrWhiteSpace(Value) && IsValid(Value)
            ? Value
            : throw new ArgumentException("Tracking code is not valid", nameof(Value));

    public static implicit operator string(TrackingCode code) => code.Value;
    public static explicit operator TrackingCode(string code) => new(code);
    public override string ToString() => Value;

    // 9400110200882960023456   // USPS Standard Tracking             // ^(\d{20,22})$
    // CP123456789US            // USPS International                 // ^[A-Z]{2}\d{9}[A-Z]{2}$
    // 1Z999AA10123456784       // UPS Tracking                       // ^1Z[A-Z0-9]{16}$
    // 123456789012             // FedEx Standard Tracking            // ^\d{12}$
    // 961102098765432          // FedEx Ground Services              // ^\d{15}$
    // 1234567890               // DHL Express Tracking               // ^\d{10}$
    // JD123456789US            // DHL International Tracking         // ^[A-Z]{2}\d{9}[A-Z]{2}$
    // TBA123456789             // Amazon Logistics Tracking          // ^TBA[A-Z0-9]{10}$
    // C12345678901234          // OnTrac Tracking                    // ^C\d{14}$
    // CP123456789CN            // China Post Tracking                // ^[A-Z]{2}\d{9}CN$
    // 1234567890123456         // Canada Post Domestic Tracking      // ^\d{16}$
    // LB123456789CA            // Canada Post International Tracking // ^[A-Z]{2}\d{9}CA$
    // AA123456789GB            // Royal Mail International Tracking  // ^[A-Z]{2}\d{9}GB$
    // ABCDEFGHI123456789       // Generic Third-Party Tracking       // ^[A-Z0-9]{9,40}$
    public static bool IsValid(string trackingCode)
    {
        var isKnown = KnownProvidersPattern().IsMatch(trackingCode);
        var isGeneric = GenericProviderPattern().IsMatch(trackingCode);
        return isKnown || isGeneric;
    }

    public static TrackingCode New()
    {
        var a = Random.Shared.NextInt64(0, long.MaxValue);
        var b = Random.Shared.NextInt64(0, long.MaxValue);

        var code = $"{a}{b}"[..21];
        return new TrackingCode($"GE{code}SGW");
    }

    [GeneratedRegex(@"^(\d{20,22}|\d{16}|\d{15}|C\d{14}|\d{14}|\d{12}|\d{10}|TBA[A-Z0-9]{10}|[A-Z]{2}\d{9}[A-Z]{2}|1Z[A-Z0-9]{16})$")]
    public static partial Regex KnownProvidersPattern();

    [GeneratedRegex("[A-Z0-9]{9,40}")]
    private static partial Regex GenericProviderPattern();
}