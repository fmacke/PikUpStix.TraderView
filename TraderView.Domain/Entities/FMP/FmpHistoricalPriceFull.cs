using System.Text.Json.Serialization;

namespace TraderView.Domain.Entities.FMP
{
    /// <summary>
    /// Represents full historical price time-series payload from FMP (/v3/historical-price-full/{symbol}).
    /// </summary>
    public class FmpHistoricalPriceFull
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("historical")]
        public List<FmpHistoricalBarDto> Historical { get; set; } = new();
    }
}