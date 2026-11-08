using System.Text.Json.Serialization;

namespace TraderView.Domain.Entities.FMP
{
    /// <summary>
    /// Represents consensus analyst estimates from FMP (/v3/analyst-estimates).
    /// </summary>
    public class FmpAnalystEstimatesDto
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;

        [JsonPropertyName("estimatedEpsAvg")]
        public decimal EstimatedEpsAvg { get; set; }

        [JsonPropertyName("estimatedEpsHigh")]
        public decimal EstimatedEpsHigh { get; set; }

        [JsonPropertyName("estimatedEpsLow")]
        public decimal EstimatedEpsLow { get; set; }

        [JsonPropertyName("estimatedEpsLowAvg")]
        public decimal EstimatedEpsLowAvg { get; set; }

        [JsonPropertyName("numberAnalystEstimatedEps")]
        public int NumberAnalystEstimatedEps { get; set; }

        [JsonPropertyName("estimatedRevenueAvg")]
        public long EstimatedRevenueAvg { get; set; }

        [JsonPropertyName("estimatedRevenueHigh")]
        public long EstimatedRevenueHigh { get; set; }

        [JsonPropertyName("estimatedRevenueLow")]
        public long EstimatedRevenueLow { get; set; }

        [JsonPropertyName("numberAnalystEstimatedRevenue")]
        public int NumberAnalystEstimatedRevenue { get; set; }
    }
}