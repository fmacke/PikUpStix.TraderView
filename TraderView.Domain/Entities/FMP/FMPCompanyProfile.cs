using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TraderView.Domain.Entities.FMP
{
    /// <summary>
    /// Represents the company profile response from FMP (/v3/profile).
    /// </summary>
    public class FmpCompanyProfile
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("beta")]
        public decimal? Beta { get; set; }

        [JsonPropertyName("volAvg")]
        public long VolAvg { get; set; }

        [JsonPropertyName("mktCap")]
        public long MarketCap { get; set; }

        [JsonPropertyName("lastDiv")]
        public decimal? LastDiv { get; set; }

        [JsonPropertyName("range")]
        public string Range { get; set; } = string.Empty;

        [JsonPropertyName("changes")]
        public decimal? Changes { get; set; }

        [JsonPropertyName("companyName")]
        public string CompanyName { get; set; } = string.Empty;

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("cik")]
        public string Cik { get; set; } = string.Empty;

        [JsonPropertyName("isin")]
        public string Isin { get; set; } = string.Empty;

        [JsonPropertyName("cusip")]
        public string Cusip { get; set; } = string.Empty;

        [JsonPropertyName("exchange")]
        public string Exchange { get; set; } = string.Empty;

        [JsonPropertyName("exchangeShortName")]
        public string ExchangeShortName { get; set; } = string.Empty;

        [JsonPropertyName("industry")]
        public string Industry { get; set; } = string.Empty;

        [JsonPropertyName("website")]
        public string Website { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("ceo")]
        public string Ceo { get; set; } = string.Empty;

        [JsonPropertyName("sector")]
        public string Sector { get; set; } = string.Empty;

        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;

        [JsonPropertyName("fullTimeEmployees")]
        public string FullTimeEmployees { get; set; } = string.Empty;

        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;

        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;

        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;

        [JsonPropertyName("zip")]
        public string Zip { get; set; } = string.Empty;

        [JsonPropertyName("image")]
        public string Image { get; set; } = string.Empty;

        [JsonPropertyName("ipoDate")]
        public string IpoDate { get; set; } = string.Empty;

        [JsonPropertyName("isEtf")]
        public bool IsEtf { get; set; }

        [JsonPropertyName("isActivelyTrading")]
        public bool IsActivelyTrading { get; set; }
    }

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

    public class FmpHistoricalBarDto
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;

        [JsonPropertyName("open")]
        public double Open { get; set; }

        [JsonPropertyName("high")]
        public double High { get; set; }

        [JsonPropertyName("low")]
        public double Low { get; set; }

        [JsonPropertyName("close")]
        public double Close { get; set; }

        [JsonPropertyName("adjClose")]
        public double AdjClose { get; set; }

        [JsonPropertyName("volume")]
        public long Volume { get; set; }

        [JsonPropertyName("unadjustedVolume")]
        public long UnadjustedVolume { get; set; }

        [JsonPropertyName("change")]
        public double Change { get; set; }

        [JsonPropertyName("changePercent")]
        public double ChangePercent { get; set; }

        [JsonPropertyName("vwap")]
        public double? Vwap { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("changeOverTime")]
        public double ChangeOverTime { get; set; }
    }

    /// <summary>
    /// Represents earnings calendar events from FMP (/v3/historical/earning_calendar/{symbol} or /v3/earning_calendar).
    /// </summary>
    public class FmpEarningsCalendarDto
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("eps")]
        public decimal? Eps { get; set; }

        [JsonPropertyName("epsEstimated")]
        public decimal? EpsEstimated { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; } = string.Empty; // e.g. "bmo" (Before market open) or "amc"

        [JsonPropertyName("revenue")]
        public long? Revenue { get; set; }

        [JsonPropertyName("revenueEstimated")]
        public long? RevenueEstimated { get; set; }

        [JsonPropertyName("fiscalDateEnding")]
        public string FiscalDateEnding { get; set; } = string.Empty;

        [JsonPropertyName("updatedFromDate")]
        public string UpdatedFromDate { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents consensus analyst estimates from FMP (/v3/analyst-estimates).
    /// </summary>
    public class FmpAnalystEstimatesDto
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;

        [JsonPropertyName("estimatedEpsAvg")]
        public decimal EstimatedEpsAvg { get; set; }

        [JsonPropertyName("estimatedEpsHigh")]
        public decimal EstimatedEpsHigh { get; set; }

        [JsonPropertyName("estimatedEpsLow")]
        public decimal EstimatedEpsLow { get; set; }

        [JsonPropertyName("estimatedEpsLowAvg")]
        public decimal EstimatedEpsLowAvg { get; set; }

        [JsonPropertyName("numberAnalystEstimatedEps")]
        public int NumberAnalystEstimatedEps { get; set; }

        [JsonPropertyName("estimatedRevenueAvg")]
        public long EstimatedRevenueAvg { get; set; }

        [JsonPropertyName("estimatedRevenueHigh")]
        public long EstimatedRevenueHigh { get; set; }

        [JsonPropertyName("estimatedRevenueLow")]
        public long EstimatedRevenueLow { get; set; }

        [JsonPropertyName("numberAnalystEstimatedRevenue")]
        public int NumberAnalystEstimatedRevenue { get; set; }
    }

    /// <summary>
    /// Represents institutional holders breakdown from FMP (/v3/institutional-holder/{symbol}).
    /// </summary>
    public class FmpInstitutionalHolderDto
    {
        [JsonPropertyName("holder")]
        public string Holder { get; set; } = string.Empty;

        [JsonPropertyName("shares")]
        public long Shares { get; set; }

        [JsonPropertyName("dateReported")]
        public string DateReported { get; set; } = string.Empty;

        [JsonPropertyName("change")]
        public long Change { get; set; }

        [JsonPropertyName("weight")]
        public decimal? Weight { get; set; }
    }
}