using System.Text.Json.Serialization;

namespace TraderView.Domain.Entities.FMP
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
}
