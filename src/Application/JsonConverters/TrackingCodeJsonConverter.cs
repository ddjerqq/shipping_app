using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.ValueObjects;

namespace Application.JsonConverters;

public sealed class TrackingCodeJsonConverter : JsonConverter<TrackingCode>
{
    public override TrackingCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        (TrackingCode)reader.GetString()!;

    public override void Write(Utf8JsonWriter writer, TrackingCode value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}