using System.Globalization;
using System.Xml.Linq;
using TraderView.Application.Utils;
using TraderView.Domain.Entities;

namespace TraderView.Application.Services
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

            //report.OpenPositions = reportXml.Descendants("OpenPosition")
            //    .Select(ParseOpenPosition)
            //    .ToList();

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

        private static TradeExecution ParseTrade(XElement trade)
        {
            try{
                var tradeExecution = new TradeExecution {
                    AccountId = trade.Attribute("accountId")?.Value,
                    AcctAlias = trade.Attribute("acctAlias")?.Value,
                    Model = trade.Attribute("model")?.Value,
                    Currency = trade.Attribute("currency")?.Value,
                    FxRateToBase = ConvertToDecimal(trade.Attribute("fxRateToBase")?.Value),
                    AssetCategory = trade.Attribute("assetCategory")?.Value,
                    Symbol = trade.Attribute("symbol")?.Value,
                    Description = trade.Attribute("description")?.Value,
                    Conid = trade.Attribute("conid")?.Value,
                    SecurityIdtype = trade.Attribute("securityIDType")?.Value,
                    Cusip = trade.Attribute("cusip")?.Value,
                    Isin = trade.Attribute("isin")?.Value,
                    Figi = trade.Attribute("figi")?.Value,
                    ListingExchange = trade.Attribute("listingExchange")?.Value,
                    UnderlyingConid = trade.Attribute("underlyingConid")?.Value,
                    UnderlyingSymbol = trade.Attribute("underlyingSymbol")?.Value,
                    UnderlyingSecurityId = trade.Attribute("underlyingSecurityID")?.Value,
                    UnderlyingListingExchange = trade.Attribute("underlyingListingExchange")?.Value,
                    Issuer = trade.Attribute("issuer")?.Value,
                    IssuerCountryCode = trade.Attribute("issuerCountryCode")?.Value,
                    Multiplier = ConvertToInt(trade.Attribute("multiplier")?.Value),
                    Strike = ConvertToDecimal(trade.Attribute("strike")?.Value),
                    Expiry = trade.Attribute("expiry")?.Value,
                    PutCall = trade.Attribute("putCall")?.Value,
                    PrincipalAdjustFactor = ConvertToDecimal(trade.Attribute("principalAdjustFactor")?.Value),
                    ReportDate = DateTime.ParseExact(trade.Attribute("reportDate")?.Value, "yyyyMMdd", CultureInfo.InvariantCulture).ToLongDateString(),
                    TradeId = long.TryParse(trade.Attribute("tradeID")?.Value, out var id) ? id : 0,
                    TradeDate = DateTime.ParseExact(trade.Attribute("tradeDate")?.Value, "yyyyMMdd", CultureInfo.InvariantCulture),
                    DateTime = DateTime.ParseExact(trade.Attribute("dateTime")?.Value, "yyyyMMdd;HHmmss", CultureInfo.InvariantCulture),
                    SettleDateTarget = DateTime.ParseExact(trade.Attribute("settleDateTarget")?.Value, "yyyyMMdd", CultureInfo.InvariantCulture).ToLongDateString(),
                    TransactionType = trade.Attribute("transactionType")?.Value,
                    Exchange = trade.Attribute("exchange")?.Value,
                    Quantity = (decimal)ConvertToDecimal(trade.Attribute("quantity")?.Value),
                    TradePrice = (decimal)ConvertToDecimal(trade.Attribute("tradePrice")?.Value),
                    TradeMoney = ConvertToDecimal(trade.Attribute("tradeMoney")?.Value),
                    Proceeds = ConvertToDecimal(trade.Attribute("proceeds")?.Value),
                    Taxes = ConvertToDecimal(trade.Attribute("taxes")?.Value),
                    IbCommission = ConvertToDecimal(trade.Attribute("ibCommission")?.Value),
                    IbCommissionCurrency = trade.Attribute("ibCommissionCurrency")?.Value,
                    NetCash = ConvertToDecimal(trade.Attribute("netCash")?.Value),
                    ClosePrice = ConvertToDecimal(trade.Attribute("closePrice")?.Value),
                    OpenCloseIndicator = trade.Attribute("openCloseIndicator")?.Value,
                    Cost = ConvertToDecimal(trade.Attribute("cost")?.Value),
                    FifoPnlRealized = ConvertToDecimal(trade.Attribute("fifoPnlRealized")?.Value),
                    MtmPnl = ConvertToDecimal(trade.Attribute("mtmPnl")?.Value),
                    OrigTradePrice = ConvertToDecimal(trade.Attribute("origTradePrice")?.Value),
                    OrigTradeDate = trade.Attribute("origTradeDate")?.Value,
                    OrigTradeId = trade.Attribute("origTradeID")?.Value,
                    OrigOrderId = ConvertToLong(trade.Attribute("origOrderID")?.Value),
                    OrigTransactionId = ConvertToLong(trade.Attribute("origTransactionID")?.Value),
                    ClearingFirmId = trade.Attribute("clearingFirmID")?.Value,
                    TransactionId = ConvertToLong(trade.Attribute("transactionID")?.Value),
                    IbOrderId = ConvertToLong(trade.Attribute("ibOrderID")?.Value),
                    IbExecId = trade.Attribute("ibExecID")?.Value,
                    BrokerageOrderId = trade.Attribute("brokerageOrderID")?.Value,
                    OrderReference = trade.Attribute("orderReference")?.Value,
                    VolatilityOrderLink = trade.Attribute("volatilityOrderLink")?.Value,
                    ExchOrderId = trade.Attribute("exchOrderId")?.Value,
                    ExtExecId = trade.Attribute("extExecID")?.Value,
                    OrderTime = trade.Attribute("orderTime")?.Value,
                    OpenDateTime = trade.Attribute("openDateTime")?.Value,
                    HoldingPeriodDateTime = trade.Attribute("holdingPeriodDateTime")?.Value,
                    WhenRealized = trade.Attribute("whenRealized")?.Value,
                    WhenReopened = trade.Attribute("whenReopened")?.Value,
                    LevelOfDetail = trade.Attribute("levelOfDetail")?.Value,
                    ChangeInPrice = ConvertToDecimal(trade.Attribute("changeInPrice")?.Value),
                    ChangeInQuantity = ConvertToDecimal(trade.Attribute("changeInQuantity")?.Value),
                    OrderType = trade.Attribute("orderType")?.Value,
                    TraderId = trade.Attribute("traderID")?.Value,
                    IsApiorder = trade.Attribute("isAPIOrder")?.Value,
                    AccruedInt = ConvertToDecimal(trade.Attribute("accruedInt")?.Value),
                    SubCategory = trade.Attribute("subCategory")?.Value,
                    BuySell = trade.Attribute("buySell")?.Value,
                    InitialInvestment = ConvertToDecimal(trade.Attribute("initialInvestment")?.Value),
                    RelatedTradeId = trade.Attribute("relatedTradeID")?.Value,
                    RelatedTransactionId = trade.Attribute("relatedTransactionID")?.Value,
                    Rtn = trade.Attribute("rtn")?.Value,
                    PositionActionId = trade.Attribute("positionActionID")?.Value,
                    SerialNumber = trade.Attribute("serialNumber")?.Value,
                    DeliveryType = trade.Attribute("deliveryType")?.Value,
                    CommodityType = trade.Attribute("commodityType")?.Value,
                    Fineness = ConvertToDecimal(trade.Attribute("fineness")?.Value),
                    Weight = ConvertToDecimal(trade.Attribute("weight")?.Value),
                    Position = new Position { Id = 0, InstrumentId = 0}
                };
                return tradeExecution;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing trade: {ex.Message}");
                return null; // or handle the error as needed
            }
        }

        //private static OpenPosition ParseOpenPosition(XElement position)
        //{
        //    return new Position
        //    {
        //        AccountId = position.Attribute("accountId")?.Value,
        //        AcctAlias = position.Attribute("acctAlias")?.Value,
        //        Model = position.Attribute("model")?.Value,
        //        Currency = position.Attribute("currency")?.Value,
        //        FxRateToBase = ConvertToDecimal(position.Attribute("fxRateToBase")?.Value),
        //        AssetCategory = position.Attribute("assetCategory")?.Value,
        //        SubCategory = position.Attribute("subCategory")?.Value,
        //        Symbol = position.Attribute("symbol")?.Value,
        //        Description = position.Attribute("description")?.Value,
        //        Conid = ConvertToLong(position.Attribute("conid")?.Value),
        //        SecurityID = position.Attribute("securityID")?.Value,
        //        SecurityIDType = position.Attribute("securityIDType")?.Value,
        //        Cusip = position.Attribute("cusip")?.Value,
        //        Isin = position.Attribute("isin")?.Value,
        //        Figi = position.Attribute("figi")?.Value,
        //        ListingExchange = position.Attribute("listingExchange")?.Value,
        //        UnderlyingConid = position.Attribute("underlyingConid")?.Value,
        //        UnderlyingSymbol = position.Attribute("underlyingSymbol")?.Value,
        //        UnderlyingSecurityID = position.Attribute("underlyingSecurityID")?.Value,
        //        UnderlyingListingExchange = position.Attribute("underlyingListingExchange")?.Value,
        //        Issuer = position.Attribute("issuer")?.Value,
        //        IssuerCountryCode = position.Attribute("issuerCountryCode")?.Value,
        //        Multiplier = ConvertToInt(position.Attribute("multiplier")?.Value),
        //        Strike = ConvertToDecimal(position.Attribute("strike")?.Value),
        //        Expiry = position.Attribute("expiry")?.Value,
        //        PutCall = position.Attribute("putCall")?.Value,
        //        PrincipalAdjustFactor = ConvertToDecimal(position.Attribute("principalAdjustFactor")?.Value),
        //        ReportDate = ConvertToDate(position.Attribute("reportDate")?.Value),
        //        Quantity = Convert.ToDecimal(position.Attribute("position")?.Value),
        //        MarkPrice = ConvertToDecimal(position.Attribute("markPrice")?.Value),
        //        PositionValue = ConvertToDecimal(position.Attribute("positionValue")?.Value),
        //        OpenPrice = ConvertToDecimal(position.Attribute("openPrice")?.Value),
        //        CostBasisPrice = ConvertToDecimal(position.Attribute("costBasisPrice")?.Value),
        //        CostBasisMoney = ConvertToDecimal(position.Attribute("costBasisMoney")?.Value),
        //        PercentOfNAV = ConvertToDecimal(position.Attribute("percentOfNAV")?.Value),
        //        FifoPnlUnrealized = ConvertToDecimal(position.Attribute("fifoPnlUnrealized")?.Value),
        //        Side = position.Attribute("side")?.Value,
        //        LevelOfDetail = position.Attribute("levelOfDetail")?.Value,
        //        OpenDateTime = position.Attribute("openDateTime")?.Value,
        //        HoldingPeriodDateTime = position.Attribute("holdingPeriodDateTime")?.Value,
        //        VestingDate = ConvertToDate(position.Attribute("vestingDate")?.Value),
        //        Code = position.Attribute("code")?.Value,
        //        OriginatingOrderID = ConvertToLong(position.Attribute("originatingOrderID")?.Value),
        //        OriginatingTransactionID = ConvertToLong(position.Attribute("originatingTransactionID")?.Value),
        //        AccruedInt = ConvertToDecimal(position.Attribute("accruedInt")?.Value),
        //        SerialNumber = position.Attribute("serialNumber")?.Value,
        //        DeliveryType = position.Attribute("deliveryType")?.Value,
        //        CommodityType = position.Attribute("commodityType")?.Value,
        //        Fineness = ConvertToDecimal(position.Attribute("fineness")?.Value),
        //        Weight = ConvertToDecimal(position.Attribute("weight")?.Value)
        //    };
        //}

        private static TradeConfirm ParseTradeConfirm(XElement tradeConfirm)
        {
            return new TradeConfirm
            {
                IbExecID = tradeConfirm.Attribute("execID")?.Value,
                Symbol = tradeConfirm.Attribute("symbol")?.Value,
                TradeDate = Convert.ToDateTime(TypeConverters.ConvertToNullableDate(tradeConfirm.Attribute("tradeDate")?.Value)),
                AssetCategory = tradeConfirm.Attribute("assetCategory")?.Value,
                ListingExchange = tradeConfirm.Attribute("listingExchange").Value,
                Quantity = (decimal)ConvertToDecimal(tradeConfirm.Attribute("quantity").Value),
                TradePrice = (decimal)ConvertToDecimal(tradeConfirm.Attribute("price").Value),
                Conid = tradeConfirm.Attribute("conid")?.Value,
                Currency = tradeConfirm.Attribute("currency")?.Value,
                OrderID = ConvertToLong(tradeConfirm.Attribute("ibOrderID")?.Value),
                BuySell = tradeConfirm.Attribute("buySell")?.Value,
                TradeID = ConvertToLong(tradeConfirm.Attribute("tradeID")?.Value),
                FifoPnlRealized = 0M,
                Commission = tradeConfirm.Attribute("commission") != null ? ConvertToDecimal(tradeConfirm.Attribute("commission").Value) : 0M,
                CommissionCurrency = tradeConfirm.Attribute("commissionCurrency")?.Value,
                TradeMoney = ConvertToDecimal(tradeConfirm.Attribute("tradeMoney")?.Value),
                Proceeds = ConvertToDecimal(tradeConfirm.Attribute("proceeds")?.Value),
                Taxes = ConvertToDecimal(tradeConfirm.Attribute("taxes")?.Value),
                NetCash = ConvertToDecimal(tradeConfirm.Attribute("netCash")?.Value),
                Amount = ConvertToDecimal(tradeConfirm.Attribute("cost")?.Value),
                Notes = tradeConfirm.Attribute("notes")?.Value,
                TransactionType = tradeConfirm.Attribute("transactionType")?.Value,
                SettleDateTarget = Convert.ToDateTime(TypeConverters.ConvertToNullableDate(tradeConfirm.Attribute("settleDateTarget")?.Value)),
                OrigTradeID = tradeConfirm.Attribute("origTradeID")?.Value,
                OrigTradeDate = tradeConfirm.Attribute("origTradeDate")?.Value,
                OrigTradePrice = ConvertToDecimal(tradeConfirm.Attribute("origTradePrice")?.Value),
                OrigOrderID = ConvertToLong(tradeConfirm.Attribute("origOrderID")?.Value),
                OrigTransactionID = ConvertToLong(tradeConfirm.Attribute("origTransactionID")?.Value),
                ClearingFirmID = tradeConfirm.Attribute("clearingFirmID")?.Value,
                AccountId = tradeConfirm.Attribute("accountId")?.Value,
                SecurityIDType = tradeConfirm.Attribute("securityIDType")?.Value,
                Cusip = tradeConfirm.Attribute("cusip")?.Value,
                Isin = tradeConfirm.Attribute("isin")?.Value,
                Figi = tradeConfirm.Attribute("figi")?.Value,
                UnderlyingConid = tradeConfirm.Attribute("underlyingConid")?.Value,
                UnderlyingSymbol = tradeConfirm.Attribute("underlyingSymbol")?.Value,
                UnderlyingSecurityID = tradeConfirm.Attribute("underlyingSecurityID")?.Value,
                UnderlyingListingExchange = tradeConfirm.Attribute("underlyingListingExchange")?.Value,
                Issuer = tradeConfirm.Attribute("issuer")?.Value,
                IssuerCountryCode = tradeConfirm.Attribute("issuerCountryCode")?.Value,
                Multiplier = ConvertToInt(tradeConfirm.Attribute("multiplier")?.Value),
                Strike = ConvertToDecimal(tradeConfirm.Attribute("strike")?.Value),
                Expiry = tradeConfirm.Attribute("expiry")?.Value,
                PutCall = tradeConfirm.Attribute("putCall")?.Value,
                PrincipalAdjustFactor = ConvertToDecimal(tradeConfirm.Attribute("principalAdjustFactor")?.Value),
                DateTime = Convert.ToDateTime(TypeConverters.ConvertToNullableDate(tradeConfirm.Attribute("dateTime")?.Value)),
                Exchange = tradeConfirm.Attribute("exchange")?.Value,
                PositionId = 0
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
