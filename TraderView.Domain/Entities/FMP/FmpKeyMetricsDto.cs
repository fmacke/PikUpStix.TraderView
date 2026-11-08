using System.Text.Json.Serialization;

namespace TraderView.Domain.Entities.FMP
{
    public class FmpKeyMetricsDto
    {
        [JsonPropertyName("returnOnEquityTTM")]
        public decimal Roe { get; set; } // e.g., 0.3245 -> 32.45% (O'Neil Benchmark >= 17%)

        [JsonPropertyName("returnOnTangibleAssetsTTM")]
        public decimal ReturnOnTangibleAssets { get; set; }

        [JsonPropertyName("incomeQualityTTM")]
        public decimal IncomeQualityTTM { get; set; }

        [JsonPropertyName("operatingReturnOnAssetsTTM")]
        public decimal OperatingReturnOnAssetsTTM { get; set; }

        [JsonPropertyName("marketCap")]
        public decimal MarketCap { get; set; }

        [JsonPropertyName("enterpriseValueTTM")]
        public decimal EnterpriseValueTTM { get; set; }

        [JsonPropertyName("evToSalesTTM")]
        public decimal EvToSalesTTM { get; set; }

        [JsonPropertyName("earningsYieldTTM")]
        public decimal EarningsYieldTTM { get; set; }

        // Note: Net profit margin is not present in this metrics payload (would typically be in financial ratios)
        public decimal NetProfitMargin =>
        EnterpriseValueTTM > 0 && EvToSalesTTM > 0
            ? EarningsYieldTTM * (MarketCap / EnterpriseValueTTM) * EvToSalesTTM
            : 0m;

        [JsonPropertyName("returnOnAssetsTTM")]
        public decimal Roa { get; set; }

        [JsonPropertyName("netDebtToEBITDATTM")]
        public decimal DebtToEquity { get; set; }

        [JsonPropertyName("currentRatioTTM")]
        public decimal CurrentRatio { get; set; }

        [JsonPropertyName("freeCashFlowYieldTTM")]
        public decimal FreeCashFlowYield { get; set; }
    }
}
