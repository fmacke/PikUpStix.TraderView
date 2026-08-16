using System.Text.Json.Serialization;

namespace TraderView.Application.Models.FMP
{
    public class FmpQuarterlyIncomeStatementDto
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("period")]
        public string Period { get; set; } = string.Empty;

        [JsonPropertyName("calendarYear")]
        public string CalendarYear { get; set; } = string.Empty;

        [JsonPropertyName("revenue")]
        public decimal Revenue { get; set; }

        [JsonPropertyName("eps")]
        public decimal Eps { get; set; }

        [JsonPropertyName("epsdiluted")]
        public decimal EpsDiluted { get; set; }

        [JsonPropertyName("fillingDate")]
        public string FillingDate { get; set; } = string.Empty;
    }

    public class CanSlimCurrentQuarterMetric
    {
        public string Symbol { get; set; } = string.Empty;
        public string LatestQuarterDate { get; set; } = string.Empty;
        public decimal LatestQuarterEps { get; set; }
        public decimal PriorYearQuarterEps { get; set; }
        public decimal EpsGrowthYoYPercent { get; set; }
        public decimal RevenueGrowthYoYPercent { get; set; }
        public bool IsAccelerating { get; set; }
        public bool PassesCriteria { get; set; }
    }
}
