using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using IKBR_Report_Puller.Domain;

namespace IKBR_Report_Puller.Services
{
    public static class IKBRReportParser
    {
        public static IKBRReport ParseMainReport(XDocument reportXml)
        {
            var report = new IKBRReport();

            var flexStatement = reportXml.Descendants("FlexStatement").FirstOrDefault();
            if (flexStatement != null)
            {
                string whenGeneratedStr = flexStatement.Attribute("whenGenerated")?.Value;
                if (!string.IsNullOrEmpty(whenGeneratedStr))
                {
                    report.WhenGenerated = DateTime.ParseExact(whenGeneratedStr, "yyyyMMdd;HHmmss", CultureInfo.InvariantCulture);
                }

                report.AccountId = flexStatement.Attribute("accountId")?.Value;
            }

            report.Trades = reportXml.Descendants("Trade")
                .Select(ParseTrade)
                .ToList();

            report.OpenPositions = reportXml.Descendants("OpenPosition")
                .Select(ParseOpenPosition)
                .ToList();

            return report;
        }

        public static IKBRReport ParseTodayReport(XDocument reportXml)
        {
            var report = new IKBRReport();

            var flexStatement = reportXml.Descendants("FlexStatement").FirstOrDefault();
            if (flexStatement != null)
            {
                string whenGeneratedStr = flexStatement.Attribute("whenGenerated")?.Value;
                if (!string.IsNullOrEmpty(whenGeneratedStr))
                {
                    report.WhenGenerated = DateTime.ParseExact(whenGeneratedStr, "yyyyMMdd;HHmmss", CultureInfo.InvariantCulture);
                }

                report.AccountId = flexStatement.Attribute("accountId")?.Value;
            }

            report.TradeConfirms = reportXml.Descendants("TradeConfirm")
                .Select(ParseTradeConfirm)
                .ToList();

            return report;
        }

        private static Trade ParseTrade(XElement trade)
        {
            return new Trade
            {
                AccountId = trade.Attribute("accountId")?.Value,
                AcctAlias = trade.Attribute("acctAlias")?.Value,
                Model = trade.Attribute("model")?.Value,
                Currency = trade.Attribute("currency")?.Value,
                FxRateToBase = ConvertToDecimal(trade.Attribute("fxRateToBase")?.Value),
                AssetCategory = trade.Attribute("assetCategory")?.Value,
                Symbol = trade.Attribute("symbol")?.Value,
                Description = trade.Attribute("description")?.Value,
                Conid = trade.Attribute("conid")?.Value,
                SecurityIDType = trade.Attribute("securityIDType")?.Value,
                Cusip = trade.Attribute("cusip")?.Value,
                Isin = trade.Attribute("isin")?.Value,
                Figi = trade.Attribute("figi")?.Value,
                ListingExchange = trade.Attribute("listingExchange")?.Value,
                UnderlyingConid = trade.Attribute("underlyingConid")?.Value,
                UnderlyingSymbol = trade.Attribute("underlyingSymbol")?.Value,
                UnderlyingSecurityID = trade.Attribute("underlyingSecurityID")?.Value,
                UnderlyingListingExchange = trade.Attribute("underlyingListingExchange")?.Value,
                Issuer = trade.Attribute("issuer")?.Value,
                IssuerCountryCode = trade.Attribute("issuerCountryCode")?.Value,
                Multiplier = ConvertToInt(trade.Attribute("multiplier")?.Value),
                Strike = ConvertToDecimal(trade.Attribute("strike")?.Value),
                Expiry = trade.Attribute("expiry")?.Value,
                PutCall = trade.Attribute("putCall")?.Value,
                PrincipalAdjustFactor = ConvertToDecimal(trade.Attribute("principalAdjustFactor")?.Value),
                ReportDate = trade.Attribute("reportDate")?.Value,
                TradeID = ConvertToLong(trade.Attribute("tradeID")?.Value),
                TradeDate = trade.Attribute("tradeDate")?.Value,
                DateTime = trade.Attribute("dateTime")?.Value,
                SettleDateTarget = trade.Attribute("settleDateTarget")?.Value,
                TransactionType = trade.Attribute("transactionType")?.Value,
                Exchange = trade.Attribute("exchange")?.Value,
                Quantity = ConvertToDecimal(trade.Attribute("quantity")?.Value),
                TradePrice = ConvertToDecimal(trade.Attribute("tradePrice")?.Value),
                TradeMoney = ConvertToDecimal(trade.Attribute("tradeMoney")?.Value),
                Proceeds = ConvertToDecimal(trade.Attribute("proceeds")?.Value),
                Taxes = ConvertToDecimal(trade.Attribute("taxes")?.Value),
                IbCommission = ConvertToDecimal(trade.Attribute("ibCommission")?.Value),
                IbCommissionCurrency = trade.Attribute("ibCommissionCurrency")?.Value,
                NetCash = ConvertToDecimal(trade.Attribute("netCash")?.Value),
                ClosePrice = ConvertToDecimal(trade.Attribute("closePrice")?.Value),
                OpenCloseIndicator = trade.Attribute("openCloseIndicator")?.Value,
                Notes = trade.Attribute("notes")?.Value,
                Cost = ConvertToDecimal(trade.Attribute("cost")?.Value),
                FifoPnlRealized = ConvertToDecimal(trade.Attribute("fifoPnlRealized")?.Value),
                MtmPnl = ConvertToDecimal(trade.Attribute("mtmPnl")?.Value),
                OrigTradePrice = ConvertToDecimal(trade.Attribute("origTradePrice")?.Value),
                OrigTradeDate = trade.Attribute("origTradeDate")?.Value,
                OrigTradeID = trade.Attribute("origTradeID")?.Value,
                OrigOrderID = ConvertToLong(trade.Attribute("origOrderID")?.Value),
                OrigTransactionID = ConvertToLong(trade.Attribute("origTransactionID")?.Value),
                ClearingFirmID = trade.Attribute("clearingFirmID")?.Value,
                TransactionID = ConvertToLong(trade.Attribute("transactionID")?.Value),
                IbOrderID = ConvertToLong(trade.Attribute("ibOrderID")?.Value),
                IbExecID = trade.Attribute("ibExecID")?.Value,
                BrokerageOrderID = trade.Attribute("brokerageOrderID")?.Value,
                OrderReference = trade.Attribute("orderReference")?.Value,
                VolatilityOrderLink = trade.Attribute("volatilityOrderLink")?.Value,
                ExchOrderId = trade.Attribute("exchOrderId")?.Value,
                ExtExecID = trade.Attribute("extExecID")?.Value,
                OrderTime = trade.Attribute("orderTime")?.Value,
                OpenDateTime = trade.Attribute("openDateTime")?.Value,
                HoldingPeriodDateTime = trade.Attribute("holdingPeriodDateTime")?.Value,
                WhenRealized = trade.Attribute("whenRealized")?.Value,
                WhenReopened = trade.Attribute("whenReopened")?.Value,
                LevelOfDetail = trade.Attribute("levelOfDetail")?.Value,
                ChangeInPrice = ConvertToDecimal(trade.Attribute("changeInPrice")?.Value),
                ChangeInQuantity = ConvertToDecimal(trade.Attribute("changeInQuantity")?.Value),
                OrderType = trade.Attribute("orderType")?.Value,
                TraderID = trade.Attribute("traderID")?.Value,
                IsAPIOrder = trade.Attribute("isAPIOrder")?.Value,
                AccruedInt = ConvertToDecimal(trade.Attribute("accruedInt")?.Value),
                SubCategory = trade.Attribute("subCategory")?.Value,
                BuySell = trade.Attribute("buySell")?.Value,
                InitialInvestment = ConvertToDecimal(trade.Attribute("initialInvestment")?.Value),
                RelatedTradeID = trade.Attribute("relatedTradeID")?.Value,
                RelatedTransactionID = trade.Attribute("relatedTransactionID")?.Value,
                Rtn = trade.Attribute("rtn")?.Value,
                PositionActionID = trade.Attribute("positionActionID")?.Value,
                SerialNumber = trade.Attribute("serialNumber")?.Value,
                DeliveryType = trade.Attribute("deliveryType")?.Value,
                CommodityType = trade.Attribute("commodityType")?.Value,
                Fineness = ConvertToDecimal(trade.Attribute("fineness")?.Value),
                Weight = ConvertToDecimal(trade.Attribute("weight")?.Value)
            };
        }

        private static OpenPosition ParseOpenPosition(XElement position)
        {
            return new OpenPosition
            {
                AccountId = position.Attribute("accountId")?.Value,
                AcctAlias = position.Attribute("acctAlias")?.Value,
                Model = position.Attribute("model")?.Value,
                Currency = position.Attribute("currency")?.Value,
                FxRateToBase = ConvertToDecimal(position.Attribute("fxRateToBase")?.Value),
                AssetCategory = position.Attribute("assetCategory")?.Value,
                SubCategory = position.Attribute("subCategory")?.Value,
                Symbol = position.Attribute("symbol")?.Value,
                Description = position.Attribute("description")?.Value,
                Conid = ConvertToLong(position.Attribute("conid")?.Value),
                SecurityID = position.Attribute("securityID")?.Value,
                SecurityIDType = position.Attribute("securityIDType")?.Value,
                Cusip = position.Attribute("cusip")?.Value,
                Isin = position.Attribute("isin")?.Value,
                Figi = position.Attribute("figi")?.Value,
                ListingExchange = position.Attribute("listingExchange")?.Value,
                UnderlyingConid = position.Attribute("underlyingConid")?.Value,
                UnderlyingSymbol = position.Attribute("underlyingSymbol")?.Value,
                UnderlyingSecurityID = position.Attribute("underlyingSecurityID")?.Value,
                UnderlyingListingExchange = position.Attribute("underlyingListingExchange")?.Value,
                Issuer = position.Attribute("issuer")?.Value,
                IssuerCountryCode = position.Attribute("issuerCountryCode")?.Value,
                Multiplier = ConvertToInt(position.Attribute("multiplier")?.Value),
                Strike = ConvertToDecimal(position.Attribute("strike")?.Value),
                Expiry = position.Attribute("expiry")?.Value,
                PutCall = position.Attribute("putCall")?.Value,
                PrincipalAdjustFactor = ConvertToDecimal(position.Attribute("principalAdjustFactor")?.Value),
                ReportDate = ConvertToDate(position.Attribute("reportDate")?.Value),
                Position = ConvertToDecimal(position.Attribute("position")?.Value),
                MarkPrice = ConvertToDecimal(position.Attribute("markPrice")?.Value),
                PositionValue = ConvertToDecimal(position.Attribute("positionValue")?.Value),
                OpenPrice = ConvertToDecimal(position.Attribute("openPrice")?.Value),
                CostBasisPrice = ConvertToDecimal(position.Attribute("costBasisPrice")?.Value),
                CostBasisMoney = ConvertToDecimal(position.Attribute("costBasisMoney")?.Value),
                PercentOfNAV = ConvertToDecimal(position.Attribute("percentOfNAV")?.Value),
                FifoPnlUnrealized = ConvertToDecimal(position.Attribute("fifoPnlUnrealized")?.Value),
                Side = position.Attribute("side")?.Value,
                LevelOfDetail = position.Attribute("levelOfDetail")?.Value,
                OpenDateTime = position.Attribute("openDateTime")?.Value,
                HoldingPeriodDateTime = position.Attribute("holdingPeriodDateTime")?.Value,
                VestingDate = ConvertToDate(position.Attribute("vestingDate")?.Value),
                Code = position.Attribute("code")?.Value,
                OriginatingOrderID = ConvertToLong(position.Attribute("originatingOrderID")?.Value),
                OriginatingTransactionID = ConvertToLong(position.Attribute("originatingTransactionID")?.Value),
                AccruedInt = ConvertToDecimal(position.Attribute("accruedInt")?.Value),
                SerialNumber = position.Attribute("serialNumber")?.Value,
                DeliveryType = position.Attribute("deliveryType")?.Value,
                CommodityType = position.Attribute("commodityType")?.Value,
                Fineness = ConvertToDecimal(position.Attribute("fineness")?.Value),
                Weight = ConvertToDecimal(position.Attribute("weight")?.Value)
            };
        }

        private static TradeConfirm ParseTradeConfirm(XElement tradeConfirm)
        {
            return new TradeConfirm
            {
                ExecID = tradeConfirm.Attribute("execID")?.Value,
                Symbol = tradeConfirm.Attribute("symbol")?.Value,
                TradeDate = tradeConfirm.Attribute("tradeDate")?.Value,
                Quantity = ConvertToDecimal(tradeConfirm.Attribute("quantity")?.Value),
                Price = ConvertToDecimal(tradeConfirm.Attribute("price")?.Value),
                ConId = tradeConfirm.Attribute("conid")?.Value,
                Currency = tradeConfirm.Attribute("currency")?.Value
            };
        }

        private static decimal? ConvertToDecimal(string value)
        {
            if (decimal.TryParse(value, out var result))
            {
                return result;
            }
            return null;
        }

        private static long? ConvertToLong(string value)
        {
            if (long.TryParse(value, out var result))
            {
                return result;
            }
            return null;
        }

        private static int? ConvertToInt(string value)
        {
            if (int.TryParse(value, out var result))
            {
                return result;
            }
            return null;
        }

        private static DateTime? ConvertToDate(string value)
        {
            if (DateTime.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }
            return null;
        }
    }
}
