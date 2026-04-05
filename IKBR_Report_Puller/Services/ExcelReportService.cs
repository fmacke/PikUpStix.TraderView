using IKBR_Report_Puller.Domain;
using IKBR_Report_Puller.Interfaces;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace IKBR_Report_Puller.Services
{
    public class ExcelReportService : IExcelReportService
    {
        private readonly IDataService _dataService;
        private readonly ITradeHistoryReportService _tradeHistoryReportService;

        public ExcelReportService(IDataService dataService, ITradeHistoryReportService tradeHistoryReportService)
        {
            _dataService = dataService;
            _tradeHistoryReportService = tradeHistoryReportService;
        }

        public void CreateReport(IKBRReport report, string outputFilePath)
        {
            try
            {
                if (report == null)
                {
                    Console.WriteLine("No report data found. Skipping Excel report creation.");
                    return;
                }

                if (!report.OpenPositions.Any())
                {
                    Console.WriteLine("No open positions found in the report. Moving to historical trades.");
                }

                // Set the EPPlus license for non-commercial use
                ExcelPackage.License.SetNonCommercialPersonal("DFM");

                using (var package = new ExcelPackage())
                {
                    CreateOpenPositionsWorkSheet(package, report);
                    _tradeHistoryReportService.CreateTradeHistoryReport(_dataService.GetTradeExecutions());
                    CreateTradeHistoryWorksheet(package, _tradeHistoryReportService.TradeHistory, "Trade History");
                    CreateTradeHistoryWorksheet(package, _tradeHistoryReportService.TradeHistoryAggregated, "Trade History Aggregated");
                    CreateVisualReport(package, _tradeHistoryReportService.TradeHistoryAggregated, "Trade Report");

                    // Save the workbook
                    string whenGeneratedStr = report.WhenGenerated.ToString("yyyyMMddHHmmss");
                    string fileName = outputFilePath.Replace("[FILE_NAME]", $"{whenGeneratedStr}.xlsx");
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string filePath = Path.Combine(desktopPath, fileName);

                    package.SaveAs(new FileInfo(filePath));
                    Console.WriteLine($"Successfully created Excel report at {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred during Excel report creation: {ex.Message}");
            }
        }

        private void CreateOpenPositionsWorkSheet(ExcelPackage package, IKBRReport report)
        {
            // Create Open Positions worksheet
            var worksheet = package.Workbook.Worksheets.Add("Open Positions");

            // Add headers
            worksheet.Cells[1, 1].Value = "Account";
            worksheet.Cells[1, 2].Value = "Symbol";
            worksheet.Cells[1, 3].Value = "Date Opened";
            worksheet.Cells[1, 4].Value = "Days Opened";
            worksheet.Cells[1, 5].Value = "Quantity";
            worksheet.Cells[1, 6].Value = "Cost Price";
            worksheet.Cells[1, 7].Value = "Average Price";
            worksheet.Cells[1, 8].Value = "Value";
            worksheet.Cells[1, 9].Value = "Unrealized P/L";
            worksheet.Cells[1, 10].Value = "% Change";
            worksheet.Cells[1, 11].Value = "Current Margin";

            // Populate data
            int currentRow = 2;
            using SqlConnection connection = new SqlConnection(_dataService.ConnectionString);
            connection.Open();

            foreach (var position in report.OpenPositions)
            {
                string accountId = position.AccountId;
                string symbol = position.Symbol;
                long? conid = position.Conid;
                decimal currentPositionQuantity = position.Position ?? 0;

                worksheet.Cells[currentRow, 1].Value = accountId;
                worksheet.Cells[currentRow, 2].Value = symbol;

                // Fetch all trades for the given conid and apply FIFO logic
                var trades = new List<(DateTime tradeDate, decimal quantity, string openClose)>();
                using (SqlCommand cmd = new SqlCommand("SELECT tradeDate, quantity, openCloseIndicator FROM [dbo].[TradeExecutions] WHERE [conid] = @conid AND [accountId] = @accountId ORDER BY tradeDate ASC, dateTime ASC", connection))
                {
                    cmd.Parameters.AddWithValue("@conid", conid ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@accountId", accountId ?? (object)DBNull.Value);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            trades.Add((
                                reader.GetDateTime(0),
                                reader.GetDecimal(1),
                                reader.IsDBNull(2) ? "" : reader.GetString(2)
                            ));
                        }
                    }
                }

                var openTrades = new Queue<(DateTime tradeDate, decimal quantity)>();
                foreach (var trade in trades)
                {
                    if (trade.openClose.Contains("O")) // Opening trade
                    {
                        openTrades.Enqueue((trade.tradeDate, trade.quantity));
                    }
                    else if (trade.openClose.Contains("C")) // Closing trade
                    {
                        decimal closingQuantity = Math.Abs(trade.quantity);
                        while (closingQuantity > 0 && openTrades.Any())
                        {
                            var (openDate, openQuantity) = openTrades.Dequeue();
                            if (openQuantity > closingQuantity)
                            {
                                // Partial close, put the remainder back
                                openTrades.Enqueue((openDate, openQuantity - closingQuantity));
                                closingQuantity = 0;
                            }
                            else
                            {
                                // Full close of this opening trade
                                closingQuantity -= openQuantity;
                            }
                        }
                    }
                }

                var dateOpenedCell = worksheet.Cells[currentRow, 3];
                var daysOpenedCell = worksheet.Cells[currentRow, 4];

                // The remaining trades in openTrades are the ones making up the current position.
                // The last one is the most recent opening date based on FIFO.
                if (openTrades.Any())
                {
                    var (mostRecentOpenDate, _) = openTrades.Last();
                    dateOpenedCell.Value = mostRecentOpenDate;
                    dateOpenedCell.Style.Numberformat.Format = "yyyy-MM-dd";
                    daysOpenedCell.Formula = $"TODAY() - {dateOpenedCell.Address}";
                }

                worksheet.Cells[currentRow, 5].Value = currentPositionQuantity;
                worksheet.Cells[currentRow, 6].Value = position.CostBasisPrice ?? 0;

                var averagePriceCell = worksheet.Cells[currentRow, 7];
                worksheet.Cells[currentRow, 8].Value = position.PositionValue ?? 0;
                worksheet.Cells[currentRow, 9].Value = position.FifoPnlUnrealized ?? 0;

                var quantityCell = worksheet.Cells[currentRow, 5];
                var costPriceCell = worksheet.Cells[currentRow, 6];
                var valueCell = worksheet.Cells[currentRow, 8];
                var percentChangeCell = worksheet.Cells[currentRow, 10];
                var marginCell = worksheet.Cells[currentRow, 11];

                averagePriceCell.Formula = $"IF({quantityCell.Address}<>0,{valueCell.Address}/{quantityCell.Address},0)";
                averagePriceCell.Style.Numberformat.Format = "#,##0.00";

                percentChangeCell.Formula = $"IF({costPriceCell.Address}<>0,({averagePriceCell.Address}-{costPriceCell.Address})/{costPriceCell.Address},0)";
                percentChangeCell.Style.Numberformat.Format = "0.00%";

                marginCell.Formula = $"{valueCell.Address}-({quantityCell.Address}*{costPriceCell.Address})";
                marginCell.Style.Numberformat.Format = "#,##0.00";

                currentRow++;
            }

            // Adjust column widths if there's data
            if (worksheet.Dimension != null)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }
        }

        private void CreateTradeHistoryWorksheet(ExcelPackage package, List<HistoricalTrade> historicalData, string worksheetName)
        {
            var worksheet = package.Workbook.Worksheets.Add(worksheetName);

            // Add headers
            worksheet.Cells[1, 1].Value = "ibOrderID";
            worksheet.Cells[1, 2].Value = "Symbol";
            worksheet.Cells[1, 3].Value = "Date Opened";
            worksheet.Cells[1, 4].Value = "Date Closed";
            worksheet.Cells[1, 5].Value = "Days Open";
            worksheet.Cells[1, 6].Value = "Quantity";
            worksheet.Cells[1, 7].Value = "Cost Price";
            worksheet.Cells[1, 8].Value = "Value Price";
            worksheet.Cells[1, 9].Value = "Cost";
            worksheet.Cells[1, 10].Value = "Value";
            worksheet.Cells[1, 11].Value = "Margin";
            worksheet.Cells[1, 12].Value = "MarginPercent";

            int currentRow = 2;
            foreach (var historicalTrade in historicalData.OrderByDescending(x => x.TradeClosed))
            {
                worksheet.Cells[currentRow, 1].Value = historicalTrade.CloseIbOrderID;
                worksheet.Cells[currentRow, 2].Value = historicalTrade.Symbol;
                worksheet.Cells[currentRow, 3].Value = historicalTrade.TradeOpened;
                worksheet.Cells[currentRow, 3].Style.Numberformat.Format = "yyyy-MM-dd";
                worksheet.Cells[currentRow, 4].Value = historicalTrade.TradeClosed;
                worksheet.Cells[currentRow, 4].Style.Numberformat.Format = "yyyy-MM-dd";
                worksheet.Cells[currentRow, 5].Value = (historicalTrade.TradeClosed - historicalTrade.TradeOpened).TotalDays;
                worksheet.Cells[currentRow, 6].Value = Math.Round(historicalTrade.Quantity, 2);
                worksheet.Cells[currentRow, 7].Value = Math.Round(historicalTrade.AveragePrice, 2);
                worksheet.Cells[currentRow, 8].Value = Math.Round(historicalTrade.ClosePrice, 2);
                worksheet.Cells[currentRow, 9].Value = Math.Round(historicalTrade.TotalCost, 2);
                worksheet.Cells[currentRow, 10].Value = Math.Round(historicalTrade.MarketValue, 2);
                worksheet.Cells[currentRow, 11].Value = Math.Round(historicalTrade.RealizedPnL, 2);
                worksheet.Cells[currentRow, 11].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[currentRow, 12].Value = Math.Round(historicalTrade.RealizedPnLPercentage, 2);
                worksheet.Cells[currentRow, 12].Style.Numberformat.Format = "#,##0.00";

                currentRow++;
            }

            // Sanitize the table name to ensure it is valid
            string sanitizedTableName = worksheetName.Replace(" ", "_").Replace("-", "_").Replace("/", "_");

            // Format the data as a table
            var tableRange = worksheet.Cells[1, 1, currentRow - 1, 12];
            var table = worksheet.Tables.Add(tableRange, sanitizedTableName);
            table.TableStyle = OfficeOpenXml.Table.TableStyles.Medium9;

            // Add totals row and sum the Margin column
            table.ShowTotal = true;
            table.Columns[10].TotalsRowFunction = OfficeOpenXml.Table.RowFunctions.Sum;
            worksheet.Cells[currentRow, 11].Style.Numberformat.Format = "#,##0.00";

            // Adjust column widths
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private void CreateVisualReport(ExcelPackage package, List<HistoricalTrade> historicalData, string worksheetName)
        {
            var worksheet = package.Workbook.Worksheets.Add(worksheetName);

            // Add data for graphs
            worksheet.Cells[1, 1].Value = "Trade Date";
            worksheet.Cells[1, 2].Value = "Cumulative P/L";
            worksheet.Cells[1, 3].Value = "Profit/Loss";
            worksheet.Cells[1, 4].Value = "Win/Loss";

            decimal cumulativePnL = 0;
            int currentRow = 2;
            foreach (var trade in historicalData.OrderBy(t => t.TradeClosed))
            {
                cumulativePnL += trade.RealizedPnL;
                worksheet.Cells[currentRow, 1].Value = trade.TradeClosed;
                worksheet.Cells[currentRow, 1].Style.Numberformat.Format = "yyyy-mm-dd";
                worksheet.Cells[currentRow, 2].Value = Math.Round(cumulativePnL, 2);
                worksheet.Cells[currentRow, 3].Value = Math.Round(trade.RealizedPnL, 2);
                worksheet.Cells[currentRow, 4].Value = Math.Round(trade.RealizedPnL, 2) >= 0 ? "Win" : "Loss";
                currentRow++;
            }

            // Create Equity Curve (Line Chart)
            var equityCurveChart = worksheet.Drawings.AddChart("EquityCurve", eChartType.Line);
            equityCurveChart.Title.Text = "Equity Curve";
            equityCurveChart.SetPosition(0, 0, 5, 0);
            equityCurveChart.SetSize(800, 400);
            var equitySeries = equityCurveChart.Series.Add(worksheet.Cells[2, 2, currentRow - 1, 2], worksheet.Cells[2, 1, currentRow - 1, 1]);
            equitySeries.Header = "Cumulative P/L";

            // Create Profit/Loss Distribution (Column Chart)
            var profitLossChart = worksheet.Drawings.AddChart("ProfitLossDistribution", eChartType.ColumnClustered);
            profitLossChart.Title.Text = "Profit/Loss Distribution";
            profitLossChart.SetPosition(20, 0, 5, 0);
            profitLossChart.SetSize(800, 400);
            var profitLossSeries = profitLossChart.Series.Add(worksheet.Cells[2, 3, currentRow - 1, 3], worksheet.Cells[2, 1, currentRow - 1, 1]);
            profitLossSeries.Header = "Profit/Loss";

            // Create Win/Loss Ratio (Pie Chart)
            worksheet.Cells[1, 6].Value = "Result";
            worksheet.Cells[1, 7].Value = "Count";
            worksheet.Cells[2, 6].Value = "Win";
            worksheet.Cells[2, 7].Formula = $"COUNTIF(D2:D{currentRow - 1}, \"Win\")";
            worksheet.Cells[3, 6].Value = "Loss";
            worksheet.Cells[3, 7].Formula = $"COUNTIF(D2:D{currentRow - 1}, \"Loss\")";

            var winLossChart = worksheet.Drawings.AddChart("WinLossRatio", eChartType.Pie);
            winLossChart.Title.Text = "Win/Loss Ratio";
            winLossChart.SetPosition(40, 0, 5, 0);
            winLossChart.SetSize(800, 400);
            var winLossSeries = winLossChart.Series.Add(worksheet.Cells[2, 7, 3, 7], worksheet.Cells[2, 6, 3, 6]);
            winLossSeries.Header = "Win/Loss";

            // Adjust column widths
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }
    }

}
