using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.JsonConverters;

public sealed class CultureInfoJsonConverter : JsonConverter<CultureInfo>
{
    public override CultureInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        CultureInfo.GetCultureInfo(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, CultureInfo value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Name);
}