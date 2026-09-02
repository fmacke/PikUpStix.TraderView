using TraderView.Domain.Entities;
using TraderView.Domain.Entities.FMP;

namespace TraderView.Application.Mappers
{
    /// <summary>
    /// Builds parameter dictionaries for TradeExecution database operations
    /// </summary>
    public static class MapToSql
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
                { "@ClosePrice", tradeConfirm.TradePrice },
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
                { "@Id", trade.Id },
                { "@PositionId", trade.PositionId },
                { "@InstrumentId", trade.Position.InstrumentId },
                { "@symbol", trade.Symbol },
                { "@conID", trade.Conid },
                { "@SecurityID", trade.UnderlyingSecurityId },
                { "@tradeID", trade.TradeId },
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
                { "@transactionID", trade.TransactionId },
                { "@ibExecID", trade.IbExecId },
                { "@brokerageOrderID", trade.BrokerageOrderId },
                { "@exchOrderId", trade.ExchOrderId },
                { "@extExecID", trade.ExtExecId },
                { "@orderType", trade.OrderType },
                { "@traderID", trade.TraderId },
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
                { "@origTradeID", trade.OrigTradeId },
                { "@origOrderID", trade.OrigOrderId },
                { "@origTransactionID", trade.OrigTransactionId },
                { "@ibOrderID", trade.IbOrderId },
                { "@openDateTime", trade.OpenDateTime },
                { "@initialInvestment", trade.InitialInvestment },
                { "@accountId", trade.AccountId },
                { "@acctAlias", trade.AcctAlias },
                { "@model", trade.Model },
                { "@fxRateToBase", trade.FxRateToBase },
                { "@subCategory", trade.SubCategory },
                { "@securityIDType", trade.SecurityIdtype },
                { "@cusip", trade.Cusip },
                { "@isin", trade.Isin },
                { "@figi", trade.Figi },
                { "@listingExchange", trade.ListingExchange },
                { "@underlyingConid", trade.UnderlyingConid },
                { "@underlyingSymbol", trade.UnderlyingSymbol },
                { "@underlyingSecurityID", trade.UnderlyingSecurityId },
                { "@underlyingListingExchange", trade.UnderlyingListingExchange },
                { "@issuer", trade.Issuer },
                { "@issuerCountryCode", trade.IssuerCountryCode },
                { "@multiplier", trade.Multiplier },
                { "@relatedTradeID", trade.RelatedTradeId },
                { "@strike", trade.Strike },
                { "@reportDate", trade.ReportDate },
                { "@putCall", trade.PutCall },
                { "@principalAdjustFactor", trade.PrincipalAdjustFactor },
                { "@settleDateTarget", trade.SettleDateTarget },
                { "@tradeMoney", trade.TradeMoney },
                { "@openCloseIndicator", trade.OpenCloseIndicator },
                { "@notes", trade.Notes },
                { "@clearingFirmID", trade.ClearingFirmId },
                { "@relatedTransactionID", trade.RelatedTransactionId },
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
                { "@isAPIOrder", trade.IsApiorder },
                { "@accruedInt", trade.AccruedInt },
                { "@positionActionID", trade.PositionActionId },
                { "@serialNumber", trade.SerialNumber },
                { "@deliveryType", trade.DeliveryType },
                { "@commodityType", trade.CommodityType },
                { "@fineness", trade.Fineness },
                { "@weight", trade.Weight }
            };
        }

        public static Dictionary<string, object> GetCanSlimAnnualHistory(CanSlimCandidateAnnualHistory annualHistory)
        {
            return new Dictionary<string, object>
            {
                { "@CandidateId", annualHistory.CandidateId },
                { "@CalendarYear", annualHistory.CalendarYear },
                { "@FiscalDate", annualHistory.FiscalDate },
                { "@Revenue", annualHistory.Revenue },
                { "@NetIncome", annualHistory.NetIncome },
                { "@EpsDiluted", annualHistory.EpsDiluted },
                { "@EpsGrowthYoYPercent", annualHistory.EpsGrowthYoYpercent }
            };
        }

        public static Dictionary<string, object> GetCanSlimCandidate(CanSlimCandidate candidate)
        {
            return new Dictionary<string, object>
            {
                { "@CanSlimScreenerSnapshotId", candidate.CanSlimScreenerSnapshotId },
                { "@Symbol", candidate.Symbol },
                { "@CompanyName", candidate.CompanyName },
                { "@Price", candidate.Price },
                { "@Volume", candidate.Volume },
                { "@MarketCap", candidate.MarketCap },
                { "@Exchange", candidate.Exchange },
                { "@Sector", candidate.Sector },
                { "@Industry", candidate.Industry },
                { "@PassesBoth", candidate.PassesBoth },
                { "@EvaluationDateUtc", candidate.EvaluationDateUtc },
                { "@CurrentQuarter_LatestQuarterDate", candidate.CurrentQuarterLatestQuarterDate },
                { "@CurrentQuarter_LatestQuarterEps", candidate.CurrentQuarterLatestQuarterEps },
                { "@CurrentQuarter_PriorYearQuarterEps", candidate.CurrentQuarterPriorYearQuarterEps },
                { "@CurrentQuarter_EpsGrowthYoYPercent", candidate.CurrentQuarterEpsGrowthYoYpercent },
                { "@CurrentQuarter_RevenueGrowthYoYPercent", candidate.CurrentQuarterRevenueGrowthYoYpercent },
                { "@CurrentQuarter_IsAccelerating", candidate.CurrentQuarterIsAccelerating },
                { "@CurrentQuarter_PassesCriteria", candidate.CurrentQuarterPassesCriteria },
                { "@Annual_EpsCagr3YearPercent", candidate.AnnualEpsCagr3YearPercent },
                { "@Annual_EpsCagr5YearPercent", candidate.AnnualEpsCagr5YearPercent },
                { "@Annual_ReturnOnEquityPercent", candidate.AnnualReturnOnEquityPercent },
                { "@Annual_HasConsecutiveAnnualGrowth", candidate.AnnualHasConsecutiveAnnualGrowth },
                { "@Annual_LatestFiscalYearEps", candidate.AnnualLatestFiscalYearEps },
                { "@Annual_LatestFiscalYear", candidate.AnnualLatestFiscalYear },
                { "@Annual_PriorYear1Eps", candidate.AnnualPriorYear1Eps },
                { "@Annual_PriorYear2Eps", candidate.AnnualPriorYear2Eps },
                { "@Annual_PriorYear3Eps", candidate.AnnualPriorYear3Eps },
                { "@Annual_OperatingMarginPercent", candidate.AnnualOperatingMarginPercent },
                { "@Annual_ReturnOnAssetsPercent", candidate.AnnualReturnOnAssetsPercent },
                { "@Annual_PassesCriteria", candidate.AnnualPassesCriteria },
                { "@Annual_FundamentalGrade", candidate.AnnualFundamentalGrade },
                { "@CreatedAtUtc", DateTime.Now }
            };
        }
    }
}
