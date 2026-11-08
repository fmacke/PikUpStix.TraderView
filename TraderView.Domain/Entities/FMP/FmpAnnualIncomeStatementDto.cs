using System.Text.Json.Serialization;

namespace TraderView.Domain.Entities.FMP
{
    public class FmpAnnualIncomeStatementDto
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("fiscalYear")]
        public string CalendarYear { get; set; } = string.Empty;

        [JsonPropertyName("period")]
        public string Period { get; set; } = string.Empty; // "FY"

        [JsonPropertyName("revenue")]
        public decimal Revenue { get; set; }

        [JsonPropertyName("grossProfit")]
        public decimal GrossProfit { get; set; }

        [JsonPropertyName("operatingIncome")]
        public decimal OperatingIncome { get; set; }

        [JsonPropertyName("netIncome")]
        public decimal NetIncome { get; set; }

        [JsonPropertyName("eps")]
        public decimal Eps { get; set; }

        [JsonPropertyName("epsdiluted")]
        public decimal EpsDiluted { get; set; }

        [JsonPropertyName("fillingDate")]
        public string FillingDate { get; set; } = string.Empty;
    }
}
