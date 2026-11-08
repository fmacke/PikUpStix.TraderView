using System.Text.Json.Serialization;

namespace TraderView.Domain.Entities.FMP
{
    public class FmpHistoricalBarDto
    {
        [JsonPropertyName("symbol")]
        public string? Symbol { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;

        [JsonPropertyName("open")]
        public double Open { get; set; }

        [JsonPropertyName("high")]
        public double High { get; set; }

        [JsonPropertyName("low")]
        public double Low { get; set; }

        [JsonPropertyName("close")]
        public double Close { get; set; }

        [JsonPropertyName("adjClose")]
        public double AdjClose { get; set; }

        [JsonPropertyName("volume")]
        public long Volume { get; set; }

        [JsonPropertyName("unadjustedVolume")]
        public long UnadjustedVolume { get; set; }

        [JsonPropertyName("change")]
        public double Change { get; set; }

        [JsonPropertyName("changePercent")]
        public double ChangePercent { get; set; }

        [JsonPropertyName("vwap")]
        public double? Vwap { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("changeOverTime")]
        public double ChangeOverTime { get; set; }
    }
}