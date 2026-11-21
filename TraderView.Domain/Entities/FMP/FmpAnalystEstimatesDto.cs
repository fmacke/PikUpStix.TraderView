using System.Text.Json.Serialization;

namespace TraderView.Domain.Entities.FMP
{
    /// <summary>
    /// Represents forward-looking consensus analyst estimates from FMP (/v3/analyst-estimates).
    /// </summary>
    public class FmpAnalystEstimatesDto
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        // Revenue Estimates
        [JsonPropertyName("revenueLow")]
        public decimal RevenueLow { get; set; }

        [JsonPropertyName("revenueHigh")]
        public decimal RevenueHigh { get; set; }

        [JsonPropertyName("revenueAvg")]
        public decimal RevenueAvg { get; set; }

        // EBITDA Estimates
        [JsonPropertyName("ebitdaLow")]
        public decimal EbitdaLow { get; set; }

        [JsonPropertyName("ebitdaHigh")]
        public decimal EbitdaHigh { get; set; }

        [JsonPropertyName("ebitdaAvg")]
        public decimal EbitdaAvg { get; set; }

        // EBIT Estimates
        [JsonPropertyName("ebitLow")]
        public decimal EbitLow { get; set; }

        [JsonPropertyName("ebitHigh")]
        public decimal EbitHigh { get; set; }

        [JsonPropertyName("ebitAvg")]
        public decimal EbitAvg { get; set; }

        // Net Income Estimates
        [JsonPropertyName("netIncomeLow")]
        public decimal NetIncomeLow { get; set; }

        [JsonPropertyName("netIncomeHigh")]
        public decimal NetIncomeHigh { get; set; }

        [JsonPropertyName("netIncomeAvg")]
        public decimal NetIncomeAvg { get; set; }

        // SGA Expense Estimates
        [JsonPropertyName("sgaExpenseLow")]
        public decimal SgaExpenseLow { get; set; }

        [JsonPropertyName("sgaExpenseHigh")]
        public decimal SgaExpenseHigh { get; set; }

        [JsonPropertyName("sgaExpenseAvg")]
        public decimal SgaExpenseAvg { get; set; }

        // EPS Estimates
        [JsonPropertyName("epsAvg")]
        public decimal EpsAvg { get; set; }

        [JsonPropertyName("epsHigh")]
        public decimal EpsHigh { get; set; }

        [JsonPropertyName("epsLow")]
        public decimal EpsLow { get; set; }

        // Analyst Counts
        [JsonPropertyName("numAnalystsRevenue")]
        public int NumAnalystsRevenue { get; set; }

        [JsonPropertyName("numAnalystsEps")]
        public int NumAnalystsEps { get; set; }
    }
}
