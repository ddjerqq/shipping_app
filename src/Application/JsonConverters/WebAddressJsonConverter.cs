using Domain.ValueObjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.JsonConverters;

public sealed class WebAddressJsonConverter : JsonConverter<WebAddress>
{
    public override WebAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        (WebAddress)reader.GetString()!;

    public override void Write(Utf8JsonWriter writer, WebAddress value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString());
}