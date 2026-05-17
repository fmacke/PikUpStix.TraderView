using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IKBR_Report_Puller.Data
{
    /// <summary>
    /// Custom JSON converter for DateTime that handles the Financial Modeling Prep API date format
    /// Format: "2026-04-08 23:50:00"
    /// </summary>
    public class FmpDateTimeConverter : JsonConverter<DateTime>
    {
        private readonly string[] _formats = new[]
        {
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd'T'HH:mm:ss",
            "yyyy-MM-dd'T'HH:mm:ss.fff",
            "yyyy-MM-dd'T'HH:mm:ss.fffZ",
            "yyyy-MM-dd"
        };

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateString = reader.GetString();

            if (string.IsNullOrEmpty(dateString))
            {
                throw new JsonException("Date string is null or empty");
            }

            // Try parsing with each format
            foreach (var format in _formats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    return date;
                }
            }

            // If none of the formats work, try general parsing
            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                return parsedDate;
            }

            throw new JsonException($"Unable to parse date: {dateString}");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        }
    }
}
