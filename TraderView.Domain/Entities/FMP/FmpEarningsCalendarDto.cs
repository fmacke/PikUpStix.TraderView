using System.Text.Json.Serialization;

namespace TraderView.Domain.Entities.FMP
{
    /// <summary>
    /// Represents earnings calendar events from FMP (/v3/historical/earning_calendar/{symbol} or /v3/earning_calendar).
    /// </summary>
    public class FmpEarningsCalendarDto
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("eps")]
        public decimal? Eps { get; set; }

        [JsonPropertyName("epsEstimated")]
        public decimal? EpsEstimated { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; } = string.Empty; // e.g. "bmo" (Before market open) or "amc"

        [JsonPropertyName("revenue")]
        public long? Revenue { get; set; }

        [JsonPropertyName("revenueEstimated")]
        public long? RevenueEstimated { get; set; }

        [JsonPropertyName("fiscalDateEnding")]
        public string FiscalDateEnding { get; set; } = string.Empty;

        [JsonPropertyName("updatedFromDate")]
        public string UpdatedFromDate { get; set; } = string.Empty;
    }
}