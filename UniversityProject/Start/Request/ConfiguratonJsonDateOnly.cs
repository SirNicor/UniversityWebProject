using System.Text.Json;
using System.Text.Json.Serialization;

namespace Start.Request;

public class ConfiguratonJsonDateOnly : JsonConverter<DateOnly> 
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        DateTime parsedDateTime = DateTime.Parse(value!, null,
            System.Globalization.DateTimeStyles.RoundtripKind);
        DateOnly dateOnly = DateOnly.FromDateTime(parsedDateTime);
        return dateOnly;
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
    }
}