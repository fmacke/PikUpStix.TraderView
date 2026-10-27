using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using TraderView.Domain.Entities;
using System.Text;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Models;

namespace PikUpStix.TraderView.Services
{
    /// <summary>
    /// Provides services for generating Excel reports from trading data including open positions, trade history, and visual analytics.
    /// Implements the <see cref="IExcelReportService"/> interface.
    /// </summary>
    public class ExcelReportService : IExcelReportService
    {
        private readonly ITradingReportService _tradingReportService;
        private readonly ITradeHistoryReportService _tradeHistoryReportService;

        public ExcelReportService(
            ITradingReportService tradingReportService,
            ITradeHistoryReportService tradeHistoryReportService)
        {
            _tradingReportService = tradingReportService;
            _tradeHistoryReportService = tradeHistoryReportService;
        }

        public async Task CreateExcelFileReport(IReadOnlyList<Position> openPositions, IReadOnlyList<TradeExecution> tradeExecutions, string outputFilePath)
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
                    await CreateOpenPositionsWorkSheet(package, openPositions);                   
                    _tradeHistoryReportService.CreateTradeHistoryReport(tradeExecutions.ToList());
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

        private async Task CreateOpenPositionsWorkSheet(ExcelPackage package, IReadOnlyList<Position> openPositions)
        {
            // Prepare the report data
            var reportData = await _tradingReportService.PrepareOpenPositionReportData(openPositions);

            // Write data to worksheet
            WriteOpenPositionsToWorksheet(package, reportData);
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
                var quant = 0m;
                if (historicalTrade != null)
                {
                    if(historicalTrade.Quantity == null)
                    {
                        quant = 0;
                    }
                    else
                    {
                        quant = Math.Round(Convert.ToDecimal(historicalTrade.Quantity.Value), 2);
                    }
                }
                
                worksheet.Cells[currentRow, 1].Value = historicalTrade.IbExecId;
                worksheet.Cells[currentRow, 2].Value = historicalTrade.Symbol;
                worksheet.Cells[currentRow, 3].Value = historicalTrade.TradeOpened;
                worksheet.Cells[currentRow, 3].Style.Numberformat.Format = "yyyy-MM-dd";
                worksheet.Cells[currentRow, 4].Value = historicalTrade.TradeClosed;
                worksheet.Cells[currentRow, 4].Style.Numberformat.Format = "yyyy-MM-dd";
                worksheet.Cells[currentRow, 5].Value = (historicalTrade.TradeClosed - historicalTrade.TradeOpened).TotalDays;
                worksheet.Cells[currentRow, 6].Value = quant;
                worksheet.Cells[currentRow, 7].Value = Math.Round(Convert.ToDecimal(historicalTrade.TradePrice), 2);
                worksheet.Cells[currentRow, 8].Value = Math.Round(Convert.ToDecimal(historicalTrade.ClosePrice), 2);
                worksheet.Cells[currentRow, 9].Value = Math.Round(Convert.ToDecimal(historicalTrade.TotalCost), 2);
                worksheet.Cells[currentRow, 10].Value = Math.Round(Convert.ToDecimal(historicalTrade.MarketValue), 2);
                worksheet.Cells[currentRow, 11].Value = Math.Round(Convert.ToDecimal(historicalTrade.IbCommission), 2);
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
