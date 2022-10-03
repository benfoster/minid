using System.Text.Json;
using System.Text.Json.Serialization;
namespace Minid;

public class IdJsonConverter : JsonConverter<Id>
{
    public override Id Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? encodedValue = reader.GetString();
        if (Id.TryParse(encodedValue, out Id id))
        {
            return id;
        }

        return default;
    }

    public override void Write(Utf8JsonWriter writer, Id value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}