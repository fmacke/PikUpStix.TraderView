using System;
using System.Collections.Generic;
using IKBR_Report_Puller.Domain;

namespace IKBR_Report_Puller.Data.Repositories
{
    /// <summary>
    /// Builds parameter dictionaries for OpenPosition database operations
    /// </summary>
    public static class OpenPositionParameterBuilder
    {
        public static Dictionary<string, object> GetOpenPositionParameters(DateTime whenGenerated, OpenPosition position)
        {
            return new Dictionary<string, object>
            {
                { "@PositionID", position.PositionID },
                { "@whenGenerated", whenGenerated },
                { "@accountId", position.AccountId },
                { "@acctAlias", position.AcctAlias },
                { "@model", position.Model },
                { "@currency", position.Currency },
                { "@fxRateToBase", position.FxRateToBase },
                { "@assetCategory", position.AssetCategory },
                { "@subCategory", position.SubCategory },
                { "@symbol", position.Symbol },
                { "@description", position.Description },
                { "@conid", position.Conid },
                { "@securityID", position.SecurityID },
                { "@securityIDType", position.SecurityIDType },
                { "@cusip", position.Cusip },
                { "@isin", position.Isin },
                { "@figi", position.Figi },
                { "@listingExchange", position.ListingExchange },
                { "@underlyingConid", position.UnderlyingConid },
                { "@underlyingSymbol", position.UnderlyingSymbol },
                { "@underlyingSecurityID", position.UnderlyingSecurityID },
                { "@underlyingListingExchange", position.UnderlyingListingExchange },
                { "@issuer", position.Issuer },
                { "@issuerCountryCode", position.IssuerCountryCode },
                { "@multiplier", position.Multiplier },
                { "@strike", position.Strike },
                { "@expiry", position.Expiry },
                { "@putCall", position.PutCall },
                { "@principalAdjustFactor", position.PrincipalAdjustFactor },
                { "@reportDate", position.ReportDate },
                { "@position", position.Position },
                { "@markPrice", position.MarkPrice },
                { "@positionValue", position.PositionValue },
                { "@openPrice", position.OpenPrice },
                { "@costBasisPrice", position.CostBasisPrice },
                { "@costBasisMoney", position.CostBasisMoney },
                { "@percentOfNAV", position.PercentOfNAV },
                { "@fifoPnlUnrealized", position.FifoPnlUnrealized },
                { "@side", position.Side },
                { "@levelOfDetail", position.LevelOfDetail },
                { "@openDateTime", position.OpenDateTime },
                { "@holdingPeriodDateTime", position.HoldingPeriodDateTime },
                { "@vestingDate", position.VestingDate },
                { "@code", position.Code },
                { "@originatingOrderID", position.OriginatingOrderID },
                { "@originatingTransactionID", position.OriginatingTransactionID },
                { "@accruedInt", position.AccruedInt },
                { "@serialNumber", position.SerialNumber },
                { "@deliveryType", position.DeliveryType },
                { "@commodityType", position.CommodityType },
                { "@fineness", position.Fineness },
                { "@weight", position.Weight }
            };
        }
    }
}
