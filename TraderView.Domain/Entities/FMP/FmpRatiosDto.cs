using System.Text.Json.Serialization;

namespace TraderView.Domain.Entities.FMP
{
    /// <summary>
    /// Represents trailing twelve months (TTM) financial ratios from FMP (/v3/ratios-ttm).
    /// </summary>
    public class FmpRatiosDto
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        // Profit Margins
        [JsonPropertyName("grossProfitMarginTTM")]
        public decimal GrossProfitMarginTtm { get; set; }

        [JsonPropertyName("ebitMarginTTM")]
        public decimal EbitMarginTtm { get; set; }

        [JsonPropertyName("ebitdaMarginTTM")]
        public decimal EbitdaMarginTtm { get; set; }

        [JsonPropertyName("operatingProfitMarginTTM")]
        public decimal OperatingProfitMarginTtm { get; set; }

        [JsonPropertyName("pretaxProfitMarginTTM")]
        public decimal PretaxProfitMarginTtm { get; set; }

        [JsonPropertyName("netProfitMarginTTM")]
        public decimal NetProfitMarginTtm { get; set; }

        // Turnover Ratios
        [JsonPropertyName("receivablesTurnoverTTM")]
        public decimal ReceivablesTurnoverTtm { get; set; }

        [JsonPropertyName("payablesTurnoverTTM")]
        public decimal PayablesTurnoverTtm { get; set; }

        [JsonPropertyName("inventoryTurnoverTTM")]
        public decimal InventoryTurnoverTtm { get; set; }

        [JsonPropertyName("fixedAssetTurnoverTTM")]
        public decimal FixedAssetTurnoverTtm { get; set; }

        [JsonPropertyName("assetTurnoverTTM")]
        public decimal AssetTurnoverTtm { get; set; }

        // Liquidity Ratios
        [JsonPropertyName("currentRatioTTM")]
        public decimal CurrentRatioTtm { get; set; }

        [JsonPropertyName("quickRatioTTM")]
        public decimal QuickRatioTtm { get; set; }

        [JsonPropertyName("cashRatioTTM")]
        public decimal CashRatioTtm { get; set; }

        [JsonPropertyName("workingCapitalTurnoverRatioTTM")]
        public decimal WorkingCapitalTurnoverRatioTtm { get; set; }

        // Valuation Ratios
        [JsonPropertyName("priceToEarningsRatioTTM")]
        public decimal PriceToEarningsRatioTtm { get; set; }

        [JsonPropertyName("priceToEarningsGrowthRatioTTM")]
        public decimal PriceToEarningsGrowthRatioTtm { get; set; }

        [JsonPropertyName("priceToEarningsDilutedRatioTTM")]
        public decimal PriceToEarningsDilutedRatioTtm { get; set; }

        [JsonPropertyName("priceToBookRatioTTM")]
        public decimal PriceToBookRatioTtm { get; set; }

        [JsonPropertyName("priceToSalesRatioTTM")]
        public decimal PriceToSalesRatioTtm { get; set; }

        [JsonPropertyName("priceToFreeCashFlowRatioTTM")]
        public decimal PriceToFreeCashFlowRatioTtm { get; set; }

        [JsonPropertyName("priceToOperatingCashFlowRatioTTM")]
        public decimal PriceToOperatingCashFlowRatioTtm { get; set; }

        [JsonPropertyName("priceToFairValueTTM")]
        public decimal PriceToFairValueTtm { get; set; }

        // Leverage Ratios
        [JsonPropertyName("debtToEquityRatioTTM")]
        public decimal DebtToEquityRatioTtm { get; set; }

        [JsonPropertyName("debtToAssetsRatioTTM")]
        public decimal DebtToAssetsRatioTtm { get; set; }

        [JsonPropertyName("debtToCapitalRatioTTM")]
        public decimal DebtToCapitalRatioTtm { get; set; }

        [JsonPropertyName("longTermDebtToCapitalRatioTTM")]
        public decimal LongTermDebtToCapitalRatioTtm { get; set; }

        [JsonPropertyName("financialLeverageRatioTTM")]
        public decimal FinancialLeverageRatioTtm { get; set; }

        [JsonPropertyName("debtToMarketCapTTM")]
        public decimal DebtToMarketCapTtm { get; set; }

        // Solvency Ratios
        [JsonPropertyName("solvencyRatioTTM")]
        public decimal SolvencyRatioTtm { get; set; }

        [JsonPropertyName("interestCoverageRatioTTM")]
        public decimal InterestCoverageRatioTtm { get; set; }

        // Cash Flow Ratios
        [JsonPropertyName("operatingCashFlowRatioTTM")]
        public decimal OperatingCashFlowRatioTtm { get; set; }

        [JsonPropertyName("operatingCashFlowSalesRatioTTM")]
        public decimal OperatingCashFlowSalesRatioTtm { get; set; }

        [JsonPropertyName("freeCashFlowOperatingCashFlowRatioTTM")]
        public decimal FreeCashFlowOperatingCashFlowRatioTtm { get; set; }

        [JsonPropertyName("operatingCashFlowCoverageRatioTTM")]
        public decimal OperatingCashFlowCoverageRatioTtm { get; set; }

        [JsonPropertyName("capitalExpenditureCoverageRatioTTM")]
        public decimal CapitalExpenditureCoverageRatioTtm { get; set; }

        [JsonPropertyName("dividendPaidAndCapexCoverageRatioTTM")]
        public decimal DividendPaidAndCapexCoverageRatioTtm { get; set; }

        // Dividend Ratios
        [JsonPropertyName("dividendPayoutRatioTTM")]
        public decimal DividendPayoutRatioTtm { get; set; }

        [JsonPropertyName("dividendYieldTTM")]
        public decimal DividendYieldTtm { get; set; }

        [JsonPropertyName("dividendPerShareTTM")]
        public decimal DividendPerShareTtm { get; set; }

        // Per Share Metrics
        [JsonPropertyName("revenuePerShareTTM")]
        public decimal RevenuePerShareTtm { get; set; }

        [JsonPropertyName("netIncomePerShareTTM")]
        public decimal NetIncomePerShareTtm { get; set; }

        [JsonPropertyName("operatingCashFlowPerShareTTM")]
        public decimal OperatingCashFlowPerShareTtm { get; set; }

        [JsonPropertyName("freeCashFlowPerShareTTM")]
        public decimal FreeCashFlowPerShareTtm { get; set; }

        [JsonPropertyName("bookValuePerShareTTM")]
        public decimal BookValuePerShareTtm { get; set; }

        [JsonPropertyName("tangibleBookValuePerShareTTM")]
        public decimal TangibleBookValuePerShareTtm { get; set; }

        [JsonPropertyName("shareholdersEquityPerShareTTM")]
        public decimal ShareholdersEquityPerShareTtm { get; set; }

        [JsonPropertyName("interestDebtPerShareTTM")]
        public decimal InterestDebtPerShareTtm { get; set; }

        [JsonPropertyName("cashPerShareTTM")]
        public decimal CashPerShareTtm { get; set; }

        [JsonPropertyName("capexPerShareTTM")]
        public decimal CapexPerShareTtm { get; set; }

        // Other Ratios
        [JsonPropertyName("effectiveTaxRateTTM")]
        public decimal EffectiveTaxRateTtm { get; set; }

        [JsonPropertyName("netIncomePerEBTTTM")]
        public decimal NetIncomePerEbtTtm { get; set; }

        [JsonPropertyName("ebtPerEbitTTM")]
        public decimal EbtPerEbitTtm { get; set; }

        [JsonPropertyName("enterpriseValueTTM")]
        public decimal EnterpriseValueTtm { get; set; }

        [JsonPropertyName("enterpriseValueMultipleTTM")]
        public decimal EnterpriseValueMultipleTtm { get; set; }
    }
}