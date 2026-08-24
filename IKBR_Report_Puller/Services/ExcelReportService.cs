using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using TraderView.Domain.Entities;
using System.Text;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Models;
using TraderView.Application.Utils;

namespace PikUpStix.TraderView.Services
{
    /// <summary>
    /// Provides services for generating Excel reports from trading data including open positions, trade history, and visual analytics.
    /// Implements the <see cref="IExcelReportService"/> interface.
    /// </summary>
    public class ExcelReportService : IExcelReportService
    {
        private readonly ITradeExecutionRepository _tradeExecutionRepository;
        private readonly ITradeHistoryReportService _tradeHistoryReportService;

        public ExcelReportService(
            ITradeExecutionRepository tradeExecutionRepository,
            ITradeHistoryReportService tradeHistoryReportService)
        {
            _tradeExecutionRepository = tradeExecutionRepository;
            _tradeHistoryReportService = tradeHistoryReportService;
        }

        public void CreateExcelFileReport(List<Position> openPositions, List<TradeExecution> tradeExecutions, string outputFilePath)
        {
            try
            {
                if (!openPositions.Any())
                {
                    Console.WriteLine("No open positions found in the report. Moving to historical trades.");
                }

                // Set the EPPlus license for non-commercial use
                ExcelPackage.License.SetNonCommercialPersonal("DFM");

                using (var package = new ExcelPackage())
                {
                    CreateOpenPositionsWorkSheet(package, openPositions);                   
                    _tradeHistoryReportService.CreateTradeHistoryReport(tradeExecutions);
                    CreateTradeHistoryWorksheet(package, _tradeHistoryReportService.TradeHistory, "TradeExecution History");
                    CreateTradeHistoryWorksheet(package, _tradeHistoryReportService.TradeHistoryAggregated, "TradeExecution History Aggregated");
                    CreateVisualReport(package, _tradeHistoryReportService.TradeHistoryAggregated, "TradeExecution Report");

                    // Save the workbook
                    string whenGeneratedStr = DateTime.Now.ToString("yyyyMMddHHmmss");
                    StringBuilder fileName = new StringBuilder(outputFilePath).Append("\\" + whenGeneratedStr + ".xlsx");
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string filePath = Path.Combine(desktopPath, fileName.ToString());

                    package.SaveAs(new FileInfo(filePath)); 
                    Console.WriteLine($"Successfully created Excel report at {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred during Excel report creation: {ex.Message}");
            }
        }

        private void CreateOpenPositionsWorkSheet(ExcelPackage package, List<Position> openPositions)
        {
            // Prepare the report data
            var reportData = PrepareOpenPositionReportData(openPositions);

            // Write data to worksheet
            WriteOpenPositionsToWorksheet(package, reportData);
        }

        /// <summary>
        /// Prepares the open position report data by calculating all necessary values.
        /// This method can be reused for different report formats (Excel, Web, etc.)
        /// </summary>
        /// <param name="openPositions">List of open positions to process</param>
        /// <returns>List of calculated open position report data</returns>
        public List<OpenPositionReportData> PrepareOpenPositionReportData(List<Position> openPositions)
        {
            var reportDataList = new List<OpenPositionReportData>();

            foreach (var position in openPositions)
            {
                try
                {
                    string accountId = position.TradeExecutions.First().AccountId;
                    string symbol = position.TradeExecutions.First().Symbol;
                    long? conid = TypeConverters.ConvertToLong(position.Instrument.ConId);
                    decimal currentPositionQuantity = position.TradeExecutions.Sum(x => x.Quantity);
                    decimal costBasisPrice = CalculateAverageCost(position.TradeExecutions);
                    decimal positionValue = position.LastReportedPrice * currentPositionQuantity;
                    decimal unrealizedPnL = (position.LastReportedPrice - costBasisPrice) * currentPositionQuantity;
                    DateTime? dateOpened = position.OpenDate;
                    var trades = _tradeExecutionRepository.GetTradeExecutionsByConIdAndAccount(conid, accountId);

                    var openTrades = new Queue<(DateTime tradeDate, decimal quantity)>();
                    foreach (var trade in trades)
                    {
                        if (trade.OpenCloseIndicator.Contains("O")) // Opening trade
                        {
                            openTrades.Enqueue((trade.TradeDate, trade.Quantity));
                        }
                        else if (trade.OpenCloseIndicator.Contains("C")) // Closing trade
                        {
                            decimal closingQuantity = Math.Abs(trade.Quantity);
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

                    // The remaining trades in openTrades are the ones making up the current position
                    // The last one is the most recent opening date based on FIFO
                    //if (openTrades.Any())
                    //{
                    //    var (mostRecentOpenDate, _) = openTrades.Last();
                    //    dateOpened = mostRecentOpenDate;
                    //}

                    // Calculate Days Opened
                    int? daysOpened = dateOpened.HasValue
                        ? (int)(DateTime.Today - dateOpened.Value.Date).TotalDays
                        : null;

                    // Calculate Average Price
                    decimal averagePrice = currentPositionQuantity != 0
                        ? positionValue / currentPositionQuantity
                        : 0;

                    // Calculate % Change
                    decimal percentChange = costBasisPrice != 0
                        ? (averagePrice - costBasisPrice) / costBasisPrice
                        : 0;

                    // Calculate Current Margin
                    decimal currentMargin = positionValue - (currentPositionQuantity * costBasisPrice);
                   
                    reportDataList.Add(new OpenPositionReportData
                    {
                        //PositionId = position.Id,
                        AccountId = accountId,
                        Symbol = symbol,
                        DateOpened = dateOpened,
                        DaysOpened = daysOpened,
                        Quantity = currentPositionQuantity,
                        CostPrice = costBasisPrice,
                        AveragePrice = averagePrice,
                        Value = positionValue,
                        UnrealizedPnL = unrealizedPnL,
                        PercentChange = percentChange,
                        CurrentMargin = currentMargin
                    });
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Error in PrepareOpenPositionReportData processing position (id: " + position.Id + ") with error: ", ex.Message);
                }
            }

            return reportDataList;
        }

        private decimal CalculateAverageCost(List<TradeExecution> tradeExecutions)
        {
            // this is an approximation of the average cost basis for the current open position
            decimal totalCost = 0;
            decimal totalQuantity = tradeExecutions.Where(x => x.OpenCloseIndicator.Contains("O")).Sum(x => x.Quantity);
            foreach (var tre in tradeExecutions.Where(x => x.OpenCloseIndicator.Contains("O")))
            {
                totalCost += tre.TradePrice * tre.Quantity;
            }
            return totalQuantity != 0 ? totalCost / totalQuantity : 0;
        }

        /// <summary>
        /// Writes the prepared open position report data to an Excel worksheet.
        /// </summary>
        /// <param name="package">The Excel package to write to</param>
        /// <param name="reportData">The prepared report data</param>
        private void WriteOpenPositionsToWorksheet(ExcelPackage package, List<OpenPositionReportData> reportData)
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

            foreach (var data in reportData)
            {
                worksheet.Cells[currentRow, 1].Value = data.AccountId;
                worksheet.Cells[currentRow, 2].Value = data.Symbol;

                if (data.DateOpened.HasValue)
                {
                    worksheet.Cells[currentRow, 3].Value = data.DateOpened.Value;
                    worksheet.Cells[currentRow, 3].Style.Numberformat.Format = "yyyy-MM-dd";
                }

                if (data.DaysOpened.HasValue)
                {
                    worksheet.Cells[currentRow, 4].Value = data.DaysOpened.Value;
                }

                worksheet.Cells[currentRow, 5].Value = data.Quantity;
                worksheet.Cells[currentRow, 6].Value = data.CostPrice;
                worksheet.Cells[currentRow, 6].Style.Numberformat.Format = "#,##0.00";

                worksheet.Cells[currentRow, 7].Value = data.AveragePrice;
                worksheet.Cells[currentRow, 7].Style.Numberformat.Format = "#,##0.00";

                worksheet.Cells[currentRow, 8].Value = data.Value;
                worksheet.Cells[currentRow, 8].Style.Numberformat.Format = "#,##0.00";

                worksheet.Cells[currentRow, 9].Value = data.UnrealizedPnL;
                worksheet.Cells[currentRow, 9].Style.Numberformat.Format = "#,##0.00";

                worksheet.Cells[currentRow, 10].Value = data.PercentChange;
                worksheet.Cells[currentRow, 10].Style.Numberformat.Format = "0.00%";

                worksheet.Cells[currentRow, 11].Value = data.CurrentMargin;
                worksheet.Cells[currentRow, 11].Style.Numberformat.Format = "#,##0.00";

                currentRow++;
            }

            // Adjust column widths if there's data
            if (worksheet.Dimension != null)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }
        }

        private void CreateTradeHistoryWorksheet(ExcelPackage package, List<HistoricalTrade> trades, string worksheetName)
        {
            var worksheet = package.Workbook.Worksheets.Add(worksheetName);

            // Add headers
            worksheet.Cells[1, 1].Value = "ibExecId";
            worksheet.Cells[1, 2].Value = "Symbol";
            worksheet.Cells[1, 3].Value = "Date Opened";
            worksheet.Cells[1, 4].Value = "Date Closed";
            worksheet.Cells[1, 5].Value = "Days Open";
            worksheet.Cells[1, 6].Value = "Quantity";
            worksheet.Cells[1, 7].Value = "Cost Price";
            worksheet.Cells[1, 8].Value = "Value Price";
            worksheet.Cells[1, 9].Value = "Cost";
            worksheet.Cells[1, 10].Value = "Value";
            worksheet.Cells[1, 11].Value = "IB Commission";
            worksheet.Cells[1, 12].Value = "IB Commission Currency";
            worksheet.Cells[1, 13].Value = "Margin";
            worksheet.Cells[1, 14].Value = "MarginPercent";

            int currentRow = 2;
            foreach (var historicalTrade in trades.OrderByDescending(x => x.TradeClosed))
            {
                var quant = Math.Round(historicalTrade.Quantity, 2);
                worksheet.Cells[currentRow, 1].Value = historicalTrade.IbExecID;
                worksheet.Cells[currentRow, 2].Value = historicalTrade.Symbol;
                worksheet.Cells[currentRow, 3].Value = historicalTrade.TradeOpened;
                worksheet.Cells[currentRow, 3].Style.Numberformat.Format = "yyyy-MM-dd";
                worksheet.Cells[currentRow, 4].Value = historicalTrade.TradeClosed;
                worksheet.Cells[currentRow, 4].Style.Numberformat.Format = "yyyy-MM-dd";
                worksheet.Cells[currentRow, 5].Value = (historicalTrade.TradeClosed - historicalTrade.TradeOpened).TotalDays;
                worksheet.Cells[currentRow, 6].Value = quant;
                worksheet.Cells[currentRow, 7].Value = Math.Round(historicalTrade.TradePrice, 2);
                worksheet.Cells[currentRow, 8].Value = Math.Round(historicalTrade.ClosePrice, 2);
                worksheet.Cells[currentRow, 9].Value = Math.Round(historicalTrade.TotalCost, 2);
                worksheet.Cells[currentRow, 10].Value = Math.Round(historicalTrade.MarketValue, 2);
                worksheet.Cells[currentRow, 11].Value = Math.Round(historicalTrade.IbCommission, 2);
                worksheet.Cells[currentRow, 11].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[currentRow, 12].Value = historicalTrade.IbCommissionCurrency;
                worksheet.Cells[currentRow, 13].Value = Math.Round(historicalTrade.RealizedPnL, 2);
                worksheet.Cells[currentRow, 13].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[currentRow, 14].Value = Math.Round(historicalTrade.RealizedPnLPercentage, 2);
                worksheet.Cells[currentRow, 14].Style.Numberformat.Format = "#,##0.00";
                currentRow++;
            }

            // Sanitize the table name to ensure it is valid
            string sanitizedTableName = worksheetName.Replace(" ", "_").Replace("-", "_").Replace("/", "_");

            // Format the data as a table
            var tableRange = worksheet.Cells[1, 1, currentRow - 1, 14];
            var table = worksheet.Tables.Add(tableRange, sanitizedTableName);
            table.TableStyle = OfficeOpenXml.Table.TableStyles.Medium9;

            // Add totals row and sum the Margin column
            table.ShowTotal = true;
            table.Columns[10].TotalsRowFunction = OfficeOpenXml.Table.RowFunctions.Sum;
            worksheet.Cells[currentRow, 10].Style.Numberformat.Format = "#,##0.00";
            table.Columns[12].TotalsRowFunction = OfficeOpenXml.Table.RowFunctions.Sum;
            worksheet.Cells[currentRow, 12].Style.Numberformat.Format = "#,##0.00";

            // Adjust column widths
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        private void CreateVisualReport(ExcelPackage package, List<HistoricalTrade> historicalData, string worksheetName)
        {
            var worksheet = package.Workbook.Worksheets.Add(worksheetName);

            // Add data for graphs
            worksheet.Cells[1, 1].Value = "TradeExecution Date";
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
