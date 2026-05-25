using IKBR_Report_Puller.Domain;
using IKBR_Report_Puller.Interfaces;
using System.Text;

namespace IKBR_Report_Puller.Services
{
    public class TradeHistoryService : ITradeHistoryReportService
    {
        public List<HistoricalTrade> TradeHistoryAggregated { get; set; } = new List<HistoricalTrade>();
        public List<HistoricalTrade> TradeHistory { get; set; } = new List<HistoricalTrade>();
        public List<Position> positions = new List<Position>();
        public void CreateTradeHistoryReport(List<TradeExecution> rawExecutions)
        {
            // Sort chronologically across the entire history to maintain structural FIFO consistency
            var executionGroups = rawExecutions
                .Where(e => e.Quantity != 0)
                .OrderBy(e => e.TradeDate)
                .GroupBy(e => e.Symbol);

            foreach (var group in executionGroups)
            {
                string symbol = group.Key;
                var longInventory = new Queue<ExecutionQueueItem>();
                var shortInventory = new Queue<ExecutionQueueItem>();
                

                foreach (var exec in group)
                {
                    decimal qtyRemaining = exec.Quantity;
                    decimal execPrice = exec.AveragePrice;
                    DateTime execTime = exec.TradeDate;
                    long execOrderId = exec.IbOrderID;
                    int instrumentId = exec.InstrumentId;


                    // Case A: Buy Transaction (Opens Long / Closes Short)
                    if (qtyRemaining > 0)
                    {
                        while (shortInventory.Count > 0 && qtyRemaining > 0)
                        {
                            var shortMatch = shortInventory.Peek();
                            decimal matchQty = Math.Min(shortMatch.Quantity, qtyRemaining);

                            // Calculate prorated commission for this closing execution
                            decimal closingCommission = exec.IBCommission * (matchQty / Math.Abs(exec.Quantity));
                            // Calculate prorated commission from the opening execution
                            decimal openingCommission = shortMatch.Commission * (matchQty / shortMatch.Quantity);

                            TradeHistory.Add(new HistoricalTrade
                            {
                                Symbol = symbol,
                                Quantity = -matchQty, // Kept negative to reflect original Short position orientation
                                AveragePrice = shortMatch.Price, // Short entry price
                                ClosePrice = execPrice,          // Cost to buy back and cover
                                OpenIbOrderID = shortMatch.IbOrderID,
                                CloseIbOrderID = execOrderId,
                                TradeOpened = shortMatch.Timestamp,
                                TradeClosed = execTime,
                                IBCommission = openingCommission + closingCommission,
                                IBCommissionCurrency = exec.IBCommissionCurrency,
                                InstrumentId = instrumentId
                            });

                            qtyRemaining -= matchQty;
                            shortMatch.Quantity -= matchQty;
                            shortMatch.Commission -= openingCommission;

                            if (shortMatch.Quantity == 0)
                                shortInventory.Dequeue();
                        }

                        if (qtyRemaining > 0)
                        {
                            longInventory.Enqueue(new ExecutionQueueItem
                            {
                                Timestamp = execTime,
                                Quantity = qtyRemaining,
                                Price = execPrice,
                                IbOrderID = execOrderId,
                                Commission = exec.IBCommission * (qtyRemaining / Math.Abs(exec.Quantity))
                            });
                        }
                    }
                    // Case B: Sell Transaction (Closes Long / Opens Short)
                    else
                    {
                        decimal sellQtyAbs = Math.Abs(qtyRemaining);

                        while (longInventory.Count > 0 && sellQtyAbs > 0)
                        {
                            var longMatch = longInventory.Peek();
                            decimal matchQty = Math.Min(longMatch.Quantity, sellQtyAbs);

                            // Calculate prorated commission for this closing execution
                            decimal closingCommission = exec.IBCommission * (matchQty / Math.Abs(exec.Quantity));
                            // Calculate prorated commission from the opening execution
                            decimal openingCommission = longMatch.Commission * (matchQty / longMatch.Quantity);

                            TradeHistory.Add(new HistoricalTrade
                            {
                                Symbol = symbol,
                                Quantity = matchQty, // Positive for Long positions
                                AveragePrice = longMatch.Price, // Entry buy price
                                ClosePrice = execPrice,         // Exit liquidation price
                                OpenIbOrderID = longMatch.IbOrderID,
                                CloseIbOrderID = execOrderId,
                                TradeOpened = longMatch.Timestamp,
                                TradeClosed = execTime,
                                IBCommission = openingCommission + closingCommission,
                                IBCommissionCurrency = exec.IBCommissionCurrency,
                                InstrumentId = instrumentId
                            });

                            sellQtyAbs -= matchQty;
                            longMatch.Quantity -= matchQty;
                            longMatch.Commission -= openingCommission;

                            if (longMatch.Quantity == 0)
                                longInventory.Dequeue();
                        }

                        if (sellQtyAbs > 0)
                        {
                            shortInventory.Enqueue(new ExecutionQueueItem
                            {
                                Timestamp = execTime,
                                Quantity = sellQtyAbs,
                                Price = execPrice,
                                IbOrderID = execOrderId,
                                Commission = exec.IBCommission * (sellQtyAbs / Math.Abs(exec.Quantity))
                            });
                        }
                    }
                }
            }
            // Aggregate TradeHistory by IbOrderID
            TradeHistoryAggregated = TradeHistory
                .GroupBy(trade => trade.CloseIbOrderID)
                .Select(group => new HistoricalTrade
                {
                    Symbol = group.First().Symbol,
                    AveragePrice = group.Average(trade => trade.AveragePrice), // Taking the average of average prices
                    ClosePrice = group.Average(trade => trade.ClosePrice), // Taking the average of close prices
                    OpenIbOrderID = group.First().OpenIbOrderID,
                    CloseIbOrderID = group.Key,
                    TradeOpened = group.First().TradeOpened,
                    TradeClosed = group.First().TradeClosed,
                    Currency = group.First().Currency,
                    InstrumentId = group.First().InstrumentId,
                    Quantity = group.Sum(trade => trade.Quantity),
                    SecurityId = group.First().SecurityId,
                    IBCommission = group.Sum(trade => trade.IBCommission),
                    IBCommissionCurrency = group.First().IBCommissionCurrency
                })
                .OrderByDescending(trade => trade.Quantity)
                .ToList();
        }
    }
    class ExecutionQueueItem
    {
        public DateTime Timestamp { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public long IbOrderID { get; set; }
        public decimal Commission { get; set; }
    }
}


