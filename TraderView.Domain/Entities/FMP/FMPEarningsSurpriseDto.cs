using System;
using System.Text.Json.Serialization;

namespace TraderView.Domain.Entities.FMP
{
    /// <summary>
    /// Represents the earnings surprise payload from FMP (/v3/earnings-surprises/{symbol}).
    /// Tracks historical consensus estimates vs. actual reported EPS and surprise metrics.
    /// </summary>
    public class FmpEarningsSurpriseDto
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("actualEarningResult")]
        public decimal ActualEarningResult { get; set; }

        [JsonPropertyName("estimatedEarning")]
        public decimal EstimatedEarning { get; set; }

        /// <summary>
        /// Absolute earnings surprise (Actual - Estimated).
        /// </summary>
        public decimal SurpriseAmount => ActualEarningResult - EstimatedEarning;

        /// <summary>
        /// Derived earnings surprise percentage calculation.
        /// Handles baseline calculations when estimated earnings are zero or negative.
        /// </summary>
        public decimal SurprisePercent
        {
            get;
            set;
        }
    }
}