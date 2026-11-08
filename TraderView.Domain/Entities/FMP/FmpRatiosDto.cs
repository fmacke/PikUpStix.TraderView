using System.Text.Json.Serialization;

namespace TraderView.Domain.Entities.FMP
{
    /// <summary>
    /// Represents trailing twelve months (TTM) financial ratios from FMP (/v3/ratios-ttm).
    /// </summary>
    public class FmpRatiosDto
    {
        [JsonPropertyName("dividendYieldTTM")]
        public decimal DividendYieldTtm { get; set; }

        [JsonPropertyName("dividendPerShareTTM")]
        public decimal DividendPerShareTtm { get; set; }

        [JsonPropertyName("payoutRatioTTM")]
        public decimal PayoutRatioTtm { get; set; }

        [JsonPropertyName("peRatioTTM")]
        public decimal PeRatioTtm { get; set; }

        [JsonPropertyName("priceToSalesRatioTTM")]
        public decimal PriceToSalesRatioTtm { get; set; }

        [JsonPropertyName("pocfratioTTM")]
        public decimal PocfratioTtm { get; set; }

        [JsonPropertyName("pfcfRatioTTM")]
        public decimal PfcfRatioTtm { get; set; }

        [JsonPropertyName("pbRatioTTM")]
        public decimal PbRatioTtm { get; set; }

        [JsonPropertyName("ptbRatioTTM")]
        public decimal PtbRatioTtm { get; set; }

        [JsonPropertyName("evToSalesTTM")]
        public decimal EvToSalesTtm { get; set; }

        [JsonPropertyName("enterpriseValueOverEBITDATTM")]
        public decimal EnterpriseValueOverEbitdaTtm { get; set; }

        [JsonPropertyName("evToOperatingCashFlowTTM")]
        public decimal EvToOperatingCashFlowTtm { get; set; }

        [JsonPropertyName("evToFreeCashFlowTTM")]
        public decimal EvToFreeCashFlowTtm { get; set; }

        [JsonPropertyName("earningsYieldTTM")]
        public decimal EarningsYieldTtm { get; set; }

        [JsonPropertyName("freeCashFlowYieldTTM")]
        public decimal FreeCashFlowYieldTtm { get; set; }

        [JsonPropertyName("debtToEquityTTM")]
        public decimal DebtEquityRatio { get; set; }

        [JsonPropertyName("debtToAssetsTTM")]
        public decimal DebtToAssetsTtm { get; set; }

        [JsonPropertyName("netDebtToEBITDATTM")]
        public decimal NetDebtToEbitdaTtm { get; set; }

        [JsonPropertyName("currentRatioTTM")]
        public decimal CurrentRatioTtm { get; set; }

        [JsonPropertyName("interestCoverageTTM")]
        public decimal InterestCoverageTtm { get; set; }

        [JsonPropertyName("incomeQualityTTM")]
        public decimal IncomeQualityTtm { get; set; }

        [JsonPropertyName("dividendPaidAndCapexCoverageTTM")]
        public decimal DividendPaidAndCapexCoverageTtm { get; set; }

        [JsonPropertyName("pretaxProfitMarginTTM")]
        public decimal PretaxProfitMargin { get; set; }

        [JsonPropertyName("grossProfitMarginTTM")]
        public decimal GrossProfitMarginTtm { get; set; }

        [JsonPropertyName("operatingProfitMarginTTM")]
        public decimal OperatingProfitMarginTtm { get; set; }

        [JsonPropertyName("netProfitMarginTTM")]
        public decimal NetProfitMarginTtm { get; set; }

        [JsonPropertyName("effectiveTaxRateTTM")]
        public decimal EffectiveTaxRateTtm { get; set; }

        [JsonPropertyName("returnOnAssetsTTM")]
        public decimal ReturnOnAssetsTtm { get; set; }

        [JsonPropertyName("returnOnEquityTTM")]
        public decimal ReturnOnEquityTtm { get; set; }

        [JsonPropertyName("returnOnCapitalEmployedTTM")]
        public decimal ReturnOnCapitalEmployedTtm { get; set; }
    }
}