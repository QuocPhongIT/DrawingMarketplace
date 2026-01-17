using System.Text.Json;
using System.Text.Json.Serialization;

namespace DrawingMarketplace.Api.Extensions
{
    public class VietnamDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.Parse(reader.GetString()!);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var vietnamTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTime(value, vietnamTz);
            writer.WriteStringValue(vietnamTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        }
    }

    public class VietnamNullableDateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;
            return DateTime.Parse(reader.GetString()!);
        }

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            var vietnamTz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamTime = TimeZoneInfo.ConvertTime(value.Value, vietnamTz);
            writer.WriteStringValue(vietnamTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
        }
    }
}
