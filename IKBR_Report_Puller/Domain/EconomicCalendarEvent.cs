using System;
using System.Text.Json.Serialization;
using IKBR_Report_Puller.Data;

namespace IKBR_Report_Puller.Domain
{
    /// <summary>
    /// Represents an economic calendar event from Financial Modeling Prep API
    /// </summary>
    public class EconomicCalendarEvent
    {
        [JsonPropertyName("date")]
        [JsonConverter(typeof(FmpDateTimeConverter))]
        public DateTime Date { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("event")]
        public string Event { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("previous")]
        public decimal? Previous { get; set; }

        [JsonPropertyName("estimate")]
        public decimal? Estimate { get; set; }

        [JsonPropertyName("actual")]
        public decimal? Actual { get; set; }

        [JsonPropertyName("change")]
        public decimal? Change { get; set; }

        [JsonPropertyName("impact")]
        public string Impact { get; set; }

        [JsonPropertyName("changePercentage")]
        public decimal? ChangePercentage { get; set; }

        [JsonPropertyName("unit")]
        public string Unit { get; set; }
    }
}
