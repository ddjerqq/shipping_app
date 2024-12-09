using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.JsonConverters;

public sealed class Vector3JsonConverter : JsonConverter<Vector3>
{
    public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var obj = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
        return new Vector3(obj.GetProperty("X").GetSingle(), obj.GetProperty("Y").GetSingle(), obj.GetProperty("Z").GetSingle());
    }

    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
    {
        var obj = new { value.X, value.Y, value.Z };
        JsonSerializer.Serialize(writer, obj, options);
    }
}