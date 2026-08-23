using TraderView.Application.Models.FMP;
using TraderView.Domain.Entities;

namespace TraderView.Infrastructure.Repositories
{
    /// <summary>
    /// Builds parameter dictionaries for TradeExecution database operations
    /// </summary>
    public static class TradeParameterBuilder
    {
        public static Dictionary<string, object> GetTradeConfirmationParams(TradeConfirm tradeConfirm)
        {
            return new Dictionary<string, object>
            {
                { "@positionId", tradeConfirm.PositionId },
                { "@ibOrderID", tradeConfirm.OrderID.ToString() },
                { "@ibexecID", tradeConfirm.IbExecID },
                { "@symbol", tradeConfirm.Symbol },
                { "@tradeDate", tradeConfirm.TradeDate },
                { "@dateTime", tradeConfirm.TradeDate },
                { "@quantity", tradeConfirm.Quantity },
                { "@tradePrice", tradeConfirm.TradePrice },
                { "@currency", tradeConfirm.Currency },
                { "@conid", tradeConfirm.Conid },
                { "@tradeID", tradeConfirm.TradeID },
                { "@fifoPnlRealized", tradeConfirm.FifoPnlRealized },
                { "@ibCommission", tradeConfirm.Commission },
                { "@assetCategory", tradeConfirm.AssetCategory },
                { "@description", tradeConfirm.Description },
                { "@securityIDType", tradeConfirm.SecurityIDType },
                { "@cusip", tradeConfirm.Cusip },
                { "@accountId", tradeConfirm.AccountId },
                { "@isin", tradeConfirm.Isin },
                { "@figi", tradeConfirm.Figi },
                { "@listingExchange", tradeConfirm.ListingExchange },
                { "@UnderlyingConid", tradeConfirm.UnderlyingConid },
                { "@UnderlyingSymbol", tradeConfirm.UnderlyingSymbol },
                { "@UnderlyingSecurityID", tradeConfirm.UnderlyingSecurityID },
                { "@UnderlyingListingExchange", tradeConfirm.UnderlyingListingExchange },
                { "@Issuer", tradeConfirm.Issuer },
                { "@IssuerCountryCode", tradeConfirm.IssuerCountryCode },
                { "@Multiplier", tradeConfirm.Multiplier },
                { "@Strike", tradeConfirm.Strike },
                { "@Expiry", tradeConfirm.Expiry },
                { "@PutCall", tradeConfirm.PutCall },
                { "@PrincipalAdjustFactor", tradeConfirm.PrincipalAdjustFactor },
                { "@TransactionType", tradeConfirm.TransactionType },
                { "@Exchange", tradeConfirm.Exchange },
                { "@Proceeds", tradeConfirm.Proceeds },
                { "@ibCommissionCurrency", tradeConfirm.CommissionCurrency },
                { "@NetCash", tradeConfirm.NetCash },
                { "@Cost", tradeConfirm.Amount },
                { "@OrigTradePrice", tradeConfirm.OrigTradePrice },
                { "@OrigTradeDate", tradeConfirm.OrigTradeDate },
                { "@OrigTradeID", tradeConfirm.OrigTradeID },
                { "@OrigOrderID", tradeConfirm.OrigOrderID },
                { "@OrigTransactionID", tradeConfirm.OrigTransactionID },
                { "@ClearingFirmID", tradeConfirm.ClearingFirmID },
                { "@BuySell", tradeConfirm.BuySell },
                { "@OpenCloseIndicator", tradeConfirm.OpenCloseIndicator }
            };
        }
        public static Dictionary<string, object> GetTradeExecutionParams(TradeExecution trade)
        {
            return new Dictionary<string, object>
            {
                { "@PositionId", trade.PositionId },
                { "@InstrumentId", trade.InstrumentId },
                { "@symbol", trade.Symbol },
                { "@conID", trade.Conid },
                { "@SecurityID", trade.UnderlyingSecurityID },
                { "@tradeID", trade.TradeID },
                { "@dateTime", trade.DateTime },
                { "@tradeDate", trade.TradeDate },
                { "@quantity", trade.Quantity },
                { "@tradePrice", trade.TradePrice },
                { "@ibCommission", trade.IbCommission },
                { "@ibCommissionCurrency", trade.IbCommissionCurrency },
                { "@closePrice", trade.ClosePrice },
                { "@cost", trade.Cost },
                { "@fifoPnlRealized", trade.FifoPnlRealized },
                { "@buySell", trade.BuySell },
                { "@transactionID", trade.TransactionID },
                { "@ibExecID", trade.IbExecID },
                { "@brokerageOrderID", trade.BrokerageOrderID },
                { "@exchOrderId", trade.ExchOrderId },
                { "@extExecID", trade.ExtExecID },
                { "@orderType", trade.OrderType },
                { "@traderID", trade.TraderID },
                { "@currency", trade.Currency },
                { "@description", trade.Description },
                { "@taxes", trade.Taxes },
                { "@assetCategory", trade.AssetCategory },
                { "@expiry", trade.Expiry },
                { "@transactionType", trade.TransactionType },
                { "@exchange", trade.Exchange },
                { "@proceeds", trade.Proceeds },
                { "@netCash", trade.NetCash },
                { "@mtmPnl", trade.MtmPnl },
                { "@origTradePrice", trade.OrigTradePrice },
                { "@origTradeDate", trade.OrigTradeDate },
                { "@origTradeID", trade.OrigTradeID },
                { "@origOrderID", trade.OrigOrderID },
                { "@origTransactionID", trade.OrigTransactionID },
                { "@ibOrderID", trade.IbOrderID },
                { "@openDateTime", trade.OpenDateTime },
                { "@initialInvestment", trade.InitialInvestment },
                { "@accountId", trade.AccountId },
                { "@acctAlias", trade.AcctAlias },
                { "@model", trade.Model },
                { "@fxRateToBase", trade.FxRateToBase },
                { "@subCategory", trade.SubCategory },
                { "@securityIDType", trade.SecurityIDType },
                { "@cusip", trade.Cusip },
                { "@isin", trade.Isin },
                { "@figi", trade.Figi },
                { "@listingExchange", trade.ListingExchange },
                { "@underlyingConid", trade.UnderlyingConid },
                { "@underlyingSymbol", trade.UnderlyingSymbol },
                { "@underlyingSecurityID", trade.UnderlyingSecurityID },
                { "@underlyingListingExchange", trade.UnderlyingListingExchange },
                { "@issuer", trade.Issuer },
                { "@issuerCountryCode", trade.IssuerCountryCode },
                { "@multiplier", trade.Multiplier },
                { "@relatedTradeID", trade.RelatedTradeID },
                { "@strike", trade.Strike },
                { "@reportDate", trade.ReportDate },
                { "@putCall", trade.PutCall },
                { "@principalAdjustFactor", trade.PrincipalAdjustFactor },
                { "@settleDateTarget", trade.SettleDateTarget },
                { "@tradeMoney", trade.TradeMoney },
                { "@openCloseIndicator", trade.OpenCloseIndicator },
                { "@notes", trade.Notes },
                { "@clearingFirmID", trade.ClearingFirmID },
                { "@relatedTransactionID", trade.RelatedTransactionID },
                { "@rtn", trade.Rtn },
                { "@orderReference", trade.OrderReference },
                { "@volatilityOrderLink", trade.VolatilityOrderLink },
                { "@orderTime", trade.OrderTime },
                { "@holdingPeriodDateTime", trade.HoldingPeriodDateTime },
                { "@whenRealized", trade.WhenRealized },
                { "@whenReopened", trade.WhenReopened },
                { "@levelOfDetail", trade.LevelOfDetail },
                { "@changeInPrice", trade.ChangeInPrice },
                { "@changeInQuantity", trade.ChangeInQuantity },
                { "@isAPIOrder", trade.IsAPIOrder },
                { "@accruedInt", trade.AccruedInt },
                { "@positionActionID", trade.PositionActionID },
                { "@serialNumber", trade.SerialNumber },
                { "@deliveryType", trade.DeliveryType },
                { "@commodityType", trade.CommodityType },
                { "@fineness", trade.Fineness },
                { "@weight", trade.Weight }
            };
        }

        internal static Dictionary<string, object> GetCanSlimAnnualHistory(AnnualEarningsPoint annualHistory)
        {
            return new Dictionary<string, object>
            {
                { "@CandidateId", annualHistory.CandidateId },
                { "@CalendarYear", annualHistory.CalendarYear },
                { "@FiscalDate", annualHistory.FiscalDate },
                { "@Revenue", annualHistory.Revenue },
                { "@NetIncome", annualHistory.NetIncome },
                { "@EpsDiluted", annualHistory.EpsDiluted },
                { "@EpsGrowthYoYPercent", annualHistory.EpsGrowthYoYPercent }
            };
        }

        internal static Dictionary<string, object> GetCanSlimCandidate(CanSlimCandidate candidate)
        {
            return new Dictionary<string, object>
            {
                { "@CanSlimScreenerSnapShotId", candidate.CanSlimScreenerSnapShotId },
                { "@Symbol", candidate.Symbol },
                { "@CompanyName", candidate.CompanyName },
                { "@Price", candidate.Price },
                { "@Volume", candidate.Volume },
                { "@MarketCap", candidate.MarketCap },
                { "@Exchange", candidate.Exchange },
                { "@Sector", candidate.Sector },
                { "@Industry", candidate.Industry },
                { "@PassesBoth", candidate.PassesBoth },
                { "@EvaluationDateUtc", candidate.Annual.EvaluationDateUtc },
                { "@CurrentQuarter_LatestQuarterDate", candidate.CurrentQuarter.LatestQuarterDate },
                { "@CurrentQuarter_LatestQuarterEps", candidate.CurrentQuarter.LatestQuarterEps },
                { "@CurrentQuarter_PriorYearQuarterEps", candidate.CurrentQuarter.PriorYearQuarterEps },
                { "@CurrentQuarter_EpsGrowthYoYPercent", candidate.CurrentQuarter.EpsGrowthYoYPercent },
                { "@CurrentQuarter_RevenueGrowthYoYPercent", candidate.CurrentQuarter.RevenueGrowthYoYPercent },
                { "@CurrentQuarter_IsAccelerating", candidate.CurrentQuarter.IsAccelerating },
                { "@CurrentQuarter_PassesCriteria", candidate.CurrentQuarter.PassesCriteria },
                { "@Annual_EpsCagr3YearPercent", candidate.Annual.EpsCagr3YearPercent },
                { "@Annual_EpsCagr5YearPercent", candidate.Annual.EpsCagr5YearPercent },
                { "@Annual_ReturnOnEquityPercent", candidate.Annual.ReturnOnEquityPercent },
                { "@Annual_HasConsecutiveAnnualGrowth", candidate.Annual.HasConsecutiveAnnualGrowth },
                { "@Annual_LatestFiscalYearEps", candidate.Annual.LatestFiscalYearEps },
                { "@Annual_LatestFiscalYear", candidate.Annual.LatestFiscalYear },
                { "@Annual_PriorYear1Eps", candidate.Annual.PriorYear1Eps },
                { "@Annual_PriorYear2Eps", candidate.Annual.PriorYear2Eps },
                { "@Annual_PriorYear3Eps", candidate.Annual.PriorYear3Eps },
                { "@Annual_OperatingMarginPercent", candidate.Annual.OperatingMarginPercent },
                { "@Annual_ReturnOnAssetsPercent", candidate.Annual.ReturnOnAssetsPercent },
                { "@Annual_PassesCriteria", candidate.Annual.PassesCriteria },
                { "@Annual_FundamentalGrade", candidate.Annual.FundamentalGrade },
                { "@CreatedAtUtc", DateTime.Now }
            };
        }
    }
}
