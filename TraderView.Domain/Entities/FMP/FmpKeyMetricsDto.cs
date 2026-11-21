using System.Text.Json.Serialization;

namespace TraderView.Domain.Entities.FMP
{
    public class FmpKeyMetricsDto
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("marketCap")]
        public decimal MarketCap { get; set; }

        [JsonPropertyName("enterpriseValueTTM")]
        public decimal EnterpriseValueTTM { get; set; }

        [JsonPropertyName("evToSalesTTM")]
        public decimal EvToSalesTTM { get; set; }

        [JsonPropertyName("evToOperatingCashFlowTTM")]
        public decimal EvToOperatingCashFlowTTM { get; set; }

        [JsonPropertyName("evToFreeCashFlowTTM")]
        public decimal EvToFreeCashFlowTTM { get; set; }

        [JsonPropertyName("evToEBITDATTM")]
        public decimal EvToEbitdaTTM { get; set; }

        [JsonPropertyName("netDebtToEBITDATTM")]
        public decimal NetDebtToEbitdaTTM { get; set; }

        [JsonPropertyName("currentRatioTTM")]
        public decimal CurrentRatioTTM { get; set; }

        [JsonPropertyName("incomeQualityTTM")]
        public decimal IncomeQualityTTM { get; set; }

        [JsonPropertyName("grahamNumberTTM")]
        public decimal GrahamNumberTTM { get; set; }

        [JsonPropertyName("grahamNetNetTTM")]
        public decimal GrahamNetNetTTM { get; set; }

        [JsonPropertyName("taxBurdenTTM")]
        public decimal TaxBurdenTTM { get; set; }

        [JsonPropertyName("interestBurdenTTM")]
        public decimal InterestBurdenTTM { get; set; }

        [JsonPropertyName("workingCapitalTTM")]
        public decimal WorkingCapitalTTM { get; set; }

        [JsonPropertyName("investedCapitalTTM")]
        public decimal InvestedCapitalTTM { get; set; }

        [JsonPropertyName("returnOnAssetsTTM")]
        public decimal ReturnOnAssetsTTM { get; set; }

        [JsonPropertyName("operatingReturnOnAssetsTTM")]
        public decimal OperatingReturnOnAssetsTTM { get; set; }

        [JsonPropertyName("returnOnTangibleAssetsTTM")]
        public decimal ReturnOnTangibleAssetsTTM { get; set; }

        [JsonPropertyName("returnOnEquityTTM")]
        public decimal ReturnOnEquityTTM { get; set; }

        [JsonPropertyName("returnOnInvestedCapitalTTM")]
        public decimal ReturnOnInvestedCapitalTTM { get; set; }

        [JsonPropertyName("returnOnCapitalEmployedTTM")]
        public decimal ReturnOnCapitalEmployedTTM { get; set; }

        [JsonPropertyName("earningsYieldTTM")]
        public decimal EarningsYieldTTM { get; set; }

        [JsonPropertyName("freeCashFlowYieldTTM")]
        public decimal FreeCashFlowYieldTTM { get; set; }

        [JsonPropertyName("capexToOperatingCashFlowTTM")]
        public decimal CapexToOperatingCashFlowTTM { get; set; }

        [JsonPropertyName("capexToDepreciationTTM")]
        public decimal CapexToDepreciationTTM { get; set; }

        [JsonPropertyName("capexToRevenueTTM")]
        public decimal CapexToRevenueTTM { get; set; }

        [JsonPropertyName("salesGeneralAndAdministrativeToRevenueTTM")]
        public decimal SalesGeneralAndAdministrativeToRevenueTTM { get; set; }

        [JsonPropertyName("researchAndDevelopementToRevenueTTM")]
        public decimal ResearchAndDevelopementToRevenueTTM { get; set; }

        [JsonPropertyName("stockBasedCompensationToRevenueTTM")]
        public decimal StockBasedCompensationToRevenueTTM { get; set; }

        [JsonPropertyName("intangiblesToTotalAssetsTTM")]
        public decimal IntangiblesToTotalAssetsTTM { get; set; }

        [JsonPropertyName("averageReceivablesTTM")]
        public decimal AverageReceivablesTTM { get; set; }

        [JsonPropertyName("averagePayablesTTM")]
        public decimal AveragePayablesTTM { get; set; }

        [JsonPropertyName("averageInventoryTTM")]
        public decimal AverageInventoryTTM { get; set; }

        [JsonPropertyName("daysOfSalesOutstandingTTM")]
        public decimal DaysOfSalesOutstandingTTM { get; set; }

        [JsonPropertyName("daysOfPayablesOutstandingTTM")]
        public decimal DaysOfPayablesOutstandingTTM { get; set; }

        [JsonPropertyName("daysOfInventoryOutstandingTTM")]
        public decimal DaysOfInventoryOutstandingTTM { get; set; }

        [JsonPropertyName("operatingCycleTTM")]
        public decimal OperatingCycleTTM { get; set; }

        [JsonPropertyName("cashConversionCycleTTM")]
        public decimal CashConversionCycleTTM { get; set; }

        [JsonPropertyName("freeCashFlowToEquityTTM")]
        public decimal FreeCashFlowToEquityTTM { get; set; }

        [JsonPropertyName("freeCashFlowToFirmTTM")]
        public decimal FreeCashFlowToFirmTTM { get; set; }

        [JsonPropertyName("tangibleAssetValueTTM")]
        public decimal TangibleAssetValueTTM { get; set; }

        [JsonPropertyName("netCurrentAssetValueTTM")]
        public decimal NetCurrentAssetValueTTM { get; set; }
    }
}
