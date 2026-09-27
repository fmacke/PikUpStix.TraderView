using System.Globalization;
using System.Xml.Linq;
using TraderView.Application.Mappers;
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
                .Select(MapFromXml.ParseTradeExecution)
                .ToList();

            // Parse equity summaries (EquitySummaryByReportDateInBase nodes)
            report.EquitySummaries = reportXml.Descendants("EquitySummaryByReportDateInBase")
                .Select(node =>
                {
                    static decimal ParseDec(XAttribute? attribute)
                    {
                        var value = (string?)attribute;
                        if (string.IsNullOrWhiteSpace(value)) return 0m;
                        return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0m;
                    }

                    DateTime reportDate = DateTime.MinValue;
                    var reportDateStr = node.Attribute("reportDate")?.Value;
                    if (!string.IsNullOrEmpty(reportDateStr))
                    {
                        DateTime.TryParseExact(reportDateStr, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out reportDate);
                    }

                    return new EquitySummary
                    {
                        AccountId = node.Attribute("accountId")?.Value ?? string.Empty,
                        AcctAlias = node.Attribute("acctAlias")?.Value,
                        Model = node.Attribute("model")?.Value,
                        Currency = node.Attribute("currency")?.Value ?? string.Empty,
                        ReportDate = reportDate,
                        Cash = ParseDec(node.Attribute("cash")),
                        CashLong = ParseDec(node.Attribute("cashLong")),
                        CashShort = ParseDec(node.Attribute("cashShort")),
                        Stock = ParseDec(node.Attribute("stock")),
                        StockLong = ParseDec(node.Attribute("stockLong")),
                        StockShort = ParseDec(node.Attribute("stockShort")),
                        Funds = ParseDec(node.Attribute("funds")),
                        FundsLong = ParseDec(node.Attribute("fundsLong")),
                        FundsShort = ParseDec(node.Attribute("fundsShort")),
                        DividendAccruals = ParseDec(node.Attribute("dividendAccruals")),
                        DividendAccrualsLong = ParseDec(node.Attribute("dividendAccrualsLong")),
                        DividendAccrualsShort = ParseDec(node.Attribute("dividendAccrualsShort")),
                        Total = ParseDec(node.Attribute("total")),
                        TotalLong = ParseDec(node.Attribute("totalLong")),
                        TotalShort = ParseDec(node.Attribute("totalShort")),
                        CreatedAt = DateTime.UtcNow
                    };
                })
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
                .Select(MapFromXml.ParseTradeConfirm)
                .ToList();

            return report;
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

        

        
    }
}
