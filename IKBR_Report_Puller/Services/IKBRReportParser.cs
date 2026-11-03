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
    }
}
