using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ClosedXML.Excel;
using IKBR_Report_Puller.Interfaces;
using Microsoft.Data.SqlClient;

namespace IKBR_Report_Puller.Services
{
    public class ExcelReportService : IExcelReportService
    {
        private readonly IDataService _dataService;

        public ExcelReportService(IDataService dataService)
        {
            _dataService = dataService;
        }

        public void CreateOpenPositionsReport(XDocument reportXml, string outputFilePath)
        {
            try
            {
                var flexStatement = reportXml.Descendants("FlexStatement").FirstOrDefault();
                if (flexStatement == null)
                {
                    Console.WriteLine("No FlexStatement found in the report. Skipping Excel report creation.");
                    return;
                }

                string whenGeneratedStr = flexStatement.Attribute("whenGenerated")?.Value;
                if (string.IsNullOrEmpty(whenGeneratedStr))
                {
                    Console.WriteLine("whenGenerated attribute is missing from FlexStatement. Skipping Excel report creation.");
                    return;
                }

                var openPositions = reportXml.Descendants("OpenPosition").ToList();
                if (!openPositions.Any())
                {
                    Console.WriteLine("No open positions found in the report. Skipping Excel report creation.");
                    return;
                }

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Open Positions");

                    // Add the new 'Trade History' worksheet
                    CreateTradeHistoryWorksheet(workbook, _dataService.ConnectionString);

                    // Add headers
                    worksheet.Cell(1, 1).Value = "Account";
                    worksheet.Cell(1, 2).Value = "Symbol";
                    worksheet.Cell(1, 3).Value = "Date Opened";
                    worksheet.Cell(1, 4).Value = "Days Opened";
                    worksheet.Cell(1, 5).Value = "Quantity";
                    worksheet.Cell(1, 6).Value = "Cost Price";
                    worksheet.Cell(1, 7).Value = "Average Price";
                    worksheet.Cell(1, 8).Value = "Value";
                    worksheet.Cell(1, 9).Value = "Unrealized P/L";
                    worksheet.Cell(1, 10).Value = "% Change";
                    worksheet.Cell(1, 11).Value = "Current Margin";

                    // Populate data
                    int currentRow = 2;
                    using SqlConnection connection = new SqlConnection(_dataService.ConnectionString);
                    connection.Open();

                    foreach (var position in openPositions)
                    {
                        string accountId = position.Attribute("accountId")?.Value;
                        string symbol = position.Attribute("symbol")?.Value;
                        string conid = position.Attribute("conid")?.Value;
                        decimal currentPositionQuantity = decimal.Parse(position.Attribute("position")?.Value ?? "0", CultureInfo.InvariantCulture);

                        worksheet.Cell(currentRow, 1).Value = accountId;
                        worksheet.Cell(currentRow, 2).Value = symbol;

                        // Fetch all trades for the given conid and apply FIFO logic
                        var trades = new List<(DateTime tradeDate, decimal quantity, string openClose)>();
                        using (SqlCommand cmd = new SqlCommand("SELECT tradeDate, quantity, openCloseIndicator FROM [dbo].[TradeExecutions] WHERE [conid] = @conid AND [accountId] = @accountId ORDER BY tradeDate ASC, dateTime ASC", connection))
                        {
                            cmd.Parameters.AddWithValue("@conid", conid);
                            cmd.Parameters.AddWithValue("@accountId", accountId);
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

                        var dateOpenedCell = worksheet.Cell(currentRow, 3);
                        var daysOpenedCell = worksheet.Cell(currentRow, 4);

                        // The remaining trades in openTrades are the ones making up the current position.
                        // The last one is the most recent opening date based on FIFO.
                        if (openTrades.Any())
                        {
                            var (mostRecentOpenDate, _) = openTrades.Last();
                            dateOpenedCell.Value = mostRecentOpenDate;
                            dateOpenedCell.Style.DateFormat.Format = "yyyy-MM-dd";
                            daysOpenedCell.FormulaA1 = $"TODAY() - {dateOpenedCell.Address}";
                        }


                        worksheet.Cell(currentRow, 5).Value = currentPositionQuantity;
                        worksheet.Cell(currentRow, 6).Value = decimal.Parse(position.Attribute("costBasisPrice")?.Value ?? "0", CultureInfo.InvariantCulture);
                        
                        var averagePriceCell = worksheet.Cell(currentRow, 7);
                        worksheet.Cell(currentRow, 8).Value = decimal.Parse(position.Attribute("positionValue")?.Value ?? "0", CultureInfo.InvariantCulture);
                        worksheet.Cell(currentRow, 9).Value = decimal.Parse(position.Attribute("fifoPnlUnrealized")?.Value ?? "0", CultureInfo.InvariantCulture);
                        
                        var quantityCell = worksheet.Cell(currentRow, 5);
                        var costPriceCell = worksheet.Cell(currentRow, 6);
                        var valueCell = worksheet.Cell(currentRow, 8);
                        var percentChangeCell = worksheet.Cell(currentRow, 10);
                        var marginCell = worksheet.Cell(currentRow, 11);

                        averagePriceCell.FormulaA1 = $"IF({quantityCell.Address} <> 0, {valueCell.Address} / {quantityCell.Address}, 0)";
                        averagePriceCell.Style.NumberFormat.Format = "#,##0.00";

                        percentChangeCell.FormulaA1 = $"IF({costPriceCell.Address.ToString()} <> 0, ({averagePriceCell.Address.ToString()} - {costPriceCell.Address.ToString()}) / {costPriceCell.Address.ToString()}, 0)";
                        percentChangeCell.Style.NumberFormat.Format = "0.00%";

                        marginCell.FormulaA1 = $"{valueCell.Address.ToString()} - ({quantityCell.Address.ToString()} * {costPriceCell.Address.ToString()})";
                        marginCell.Style.NumberFormat.Format = "#,##0.00";

                        currentRow++;
                    }

                    string fileName = outputFilePath.Replace("[FILE_NAME]", $"{whenGeneratedStr.Replace(";", "")}.xlsx");
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string filePath = Path.Combine(desktopPath, fileName);
                    
                    workbook.SaveAs(filePath);
                    Console.WriteLine($"Successfully created Excel report at {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred during Excel report creation: {ex.Message}");
            }
        }

        public void CreateTradeHistoryWorksheet(XLWorkbook workbook, string connectionString)
        {
            var worksheet = workbook.Worksheets.Add("Trade History");

            // Add headers
            worksheet.Cell(1, 1).Value = "ibOrderID";
            worksheet.Cell(1, 2).Value = "Symbol";
            worksheet.Cell(1, 3).Value = "Date Opened";
            worksheet.Cell(1, 4).Value = "Date Closed";
            worksheet.Cell(1, 5).Value = "Days Open";
            worksheet.Cell(1, 6).Value = "Quantity";
            worksheet.Cell(1, 7).Value = "Cost Price";
            worksheet.Cell(1, 8).Value = "Value Price";
            worksheet.Cell(1, 9).Value = "Cost";
            worksheet.Cell(1, 10).Value = "Value";
            worksheet.Cell(1, 11).Value = "Margin";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var tradesByOrderId = new Dictionary<string, List<(string symbol, DateTime tradeDate, decimal quantity, decimal price, string openClose)>>();

                using (var cmd = new SqlCommand("SELECT ibOrderID, symbol, tradeDate, quantity, tradePrice, openCloseIndicator FROM [dbo].[TradeExecutions] ORDER BY ibOrderID, tradeDate ASC, dateTime ASC", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            long? ibOrderID = reader.IsDBNull(0) ? (long?)null : reader.GetInt64(0);
                            string symbol = reader.GetString(1);
                            if (ibOrderID.HasValue && !tradesByOrderId.ContainsKey(ibOrderID.Value.ToString()))
                            {
                                tradesByOrderId[ibOrderID.Value.ToString()] = new List<(string, DateTime, decimal, decimal, string)>();
                            }
                            tradesByOrderId[ibOrderID?.ToString() ?? "NULL"].Add((
                                symbol,
                                reader.GetDateTime(2),
                                reader.GetDecimal(3),
                                reader.GetDecimal(4),
                                reader.IsDBNull(5) ? "" : reader.GetString(5)
                            ));
                        }
                    }
                }

                int currentRow = 2;var openTrades = new Queue<(DateTime tradeDate, decimal quantity, decimal price)>();
                foreach (var orderId in tradesByOrderId.Keys)
                {
                    
                    var trades = tradesByOrderId[orderId];

                    Console.WriteLine($"Number of trades for orderId {orderId}: {trades.Count}");

                    foreach (var trade in trades)
                    {
                        if (!string.IsNullOrEmpty(trade.openClose) && trade.openClose.Trim().Equals("O", StringComparison.OrdinalIgnoreCase)) // Opening trade
                        {
                            openTrades.Enqueue((trade.tradeDate, trade.quantity, trade.price));
                            Console.WriteLine($"Enqueued open trade: {trade.tradeDate}, {trade.quantity}, {trade.price}");
                        }
                        else if (!string.IsNullOrEmpty(trade.openClose) && trade.openClose.Trim().Equals("C", StringComparison.OrdinalIgnoreCase)) // Closing trade
                        {
                            decimal closingQuantity = Math.Abs(trade.quantity);
                            decimal closingPrice = trade.price;
                            DateTime closingDate = trade.tradeDate;

                            while (closingQuantity > 0 && openTrades.Any())
                            {
                                var (openDate, openQuantity, openPrice) = openTrades.Dequeue();
                                decimal quantityToClose = Math.Min(closingQuantity, openQuantity);

                                worksheet.Cell(currentRow, 1).Value = orderId;
                                worksheet.Cell(currentRow, 2).Value = trade.symbol;
                                var dateOpenedCell = worksheet.Cell(currentRow, 3);
                                dateOpenedCell.Value = openDate;
                                dateOpenedCell.Style.DateFormat.Format = "yyyy-MM-dd";

                                var dateClosedCell = worksheet.Cell(currentRow, 4);
                                dateClosedCell.Value = closingDate;
                                dateClosedCell.Style.DateFormat.Format = "yyyy-MM-dd";

                                var daysOpenCell = worksheet.Cell(currentRow, 5);
                                daysOpenCell.FormulaA1 = $"{dateClosedCell.Address} - {dateOpenedCell.Address}";

                                worksheet.Cell(currentRow, 6).Value = quantityToClose;
                                worksheet.Cell(currentRow, 7).Value = openPrice;
                                worksheet.Cell(currentRow, 8).Value = closingPrice;

                                var costCell = worksheet.Cell(currentRow, 9);
                                costCell.FormulaA1 = $"{worksheet.Cell(currentRow, 6).Address} * {worksheet.Cell(currentRow, 7).Address}";
                                costCell.Style.NumberFormat.Format = "#,##0.00";

                                var valueCell = worksheet.Cell(currentRow, 10);
                                valueCell.FormulaA1 = $"{worksheet.Cell(currentRow, 6).Address} * {worksheet.Cell(currentRow, 8).Address}";
                                valueCell.Style.NumberFormat.Format = "#,##0.00";

                                var marginCell = worksheet.Cell(currentRow, 11);
                                marginCell.FormulaA1 = $"{valueCell.Address} - {costCell.Address}";
                                marginCell.Style.NumberFormat.Format = "#,##0.00";

                                currentRow++;

                                closingQuantity -= quantityToClose;
                                if (openQuantity > quantityToClose)
                                {
                                    // Re-enqueue the remaining part of the open trade back into the same queue
                                    openTrades.Enqueue((openDate, openQuantity - quantityToClose, openPrice));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
