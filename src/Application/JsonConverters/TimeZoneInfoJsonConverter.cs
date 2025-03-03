using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.JsonConverters;

public sealed class TimeZoneInfoJsonConverter : JsonConverter<TimeZoneInfo>
{
    public override TimeZoneInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        TimeZoneInfo.FindSystemTimeZoneById(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, TimeZoneInfo value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Id);
}