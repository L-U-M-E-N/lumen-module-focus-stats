using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lumen.Modules.FocusStats.ConsoleApp {
    public record OldDataFormat {
        [JsonConverter(typeof(IntAndStringToDateTimeJsonConverter))]
        public DateTime Date { get; set; }
        public int Duration { get; set; }
    }

    public class IntAndStringToDateTimeJsonConverter : JsonConverter<DateTime> {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType == JsonTokenType.String) {
                return DateTime.Parse(reader.GetString());
            }

            return new DateTime(DateTime.UnixEpoch.Ticks + reader.GetInt64() * 10_000, DateTimeKind.Utc);
        }

        public override void Write(
            Utf8JsonWriter writer,
            DateTime dateTimeValue,
            JsonSerializerOptions options) {

            throw new NotImplementedException();
        }
    }
}