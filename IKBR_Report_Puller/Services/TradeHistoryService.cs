using IKBR_Report_Puller.Domain;
using IKBR_Report_Puller.Interfaces;

namespace IKBR_Report_Puller.Services
{
    public class TradeHistoryService : ITradeHistoryReportService
    {
        public List<HistoricalTrade> TradeHistoryAggregated { get; set; } = new List<HistoricalTrade>();
        public List<HistoricalTrade> TradeHistory { get; set; } = new List<HistoricalTrade>();
        public List<Position> positions = new List<Position>();
        public void CreateTradeHistoryReport(List<TradeExecution> tradeExecutions)
        {            
            foreach (var tradeExecution in tradeExecutions)
            {
                if (!AnyOpenPositions(tradeExecution.Symbol))
                {
                    // No open positions
                    OpenPosition(tradeExecution);
                }
                else {
                    // Update existing position
                    ModifyPosition(tradeExecution, GetPosition(tradeExecution.Symbol));
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
                    Quantity = group.Sum(trade => trade.Quantity),
                    SecurityId = group.First().SecurityId
                })
                .OrderByDescending(trade => trade.Quantity)
                .ToList();
        }

        private Position GetPosition(string symbol)
        {
            var position = positions.Where(x => x.Symbol == symbol && x.IsClosed == false).FirstOrDefault();
            if (position != null)
                return position;
            throw new Exception("No position found.");
        }

        private void ModifyPosition(TradeExecution tradeExecution, Position position)
        {
            if (tradeExecution.IsLong && position.IsShort
                || tradeExecution.IsShort && position.IsLong)
            {
                CloseOrReversePosition(tradeExecution, position);
            }
            if(tradeExecution.IsLong && position.IsLong
                || tradeExecution.IsShort && position.IsShort)
            {
                IncreasePosition(tradeExecution, position);
            }
        }
       private void CloseOrReversePosition(TradeExecution tradeExecution, Position position)
        {            
            UpdatePosition(tradeExecution, position);
            AddHistoricalTrade(position, tradeExecution);            
        }

        public void UpdatePosition(TradeExecution tradeExecution, Position position)
        {
            var existingQuantity = position.Quantity;
            var revisedQuantity = position.Quantity + tradeExecution.Quantity;
            if (revisedQuantity == 0)
            {
                // Position is fully closed
                position.IsClosed = true;
            }
            if (revisedQuantity > 0 && position.IsLong)
            {
                // Position is reduced but still long
                position.Quantity = revisedQuantity;
                position.AveragePrice = (position.AveragePrice + tradeExecution.AveragePrice) / 2m;
            }
            if (revisedQuantity < 0 && position.IsShort)
            {
                // Position is reduced but still short
                position.Quantity = revisedQuantity;
                position.AveragePrice = (position.AveragePrice + tradeExecution.AveragePrice) / 2m;
            }
            if(revisedQuantity > 0 && position.IsShort
                || revisedQuantity < 0 && position.IsLong)
            {
                // Position is reversed 
                position.IsClosed = true;
                positions.Add(new Position()
                {
                    Symbol = position.Symbol,
                    AveragePrice = tradeExecution.AveragePrice,
                    Quantity = revisedQuantity,
                    IsClosed = false,
                    IbOrderID = tradeExecution.IbOrderID,
                    TradeDate = tradeExecution.TradeDate
                });
            }
        }

        private void AddHistoricalTrade(Position position, TradeExecution tradeExecution)
        {
            var tradeQuantity = Math.Min(position.Quantity, Math.Abs(tradeExecution.Quantity));
            TradeHistory.Add(new HistoricalTrade
            {
                Symbol = position.Symbol,
                AveragePrice = position.AveragePrice,
                ClosePrice = tradeExecution.AveragePrice,
                //RealizedPnL = CalculateRealizedPnL(position.TradeType, position.AveragePrice, tradeExecution.AveragePrice, tradeQuantity),
                OpenIbOrderID = position.IbOrderID,
                CloseIbOrderID = tradeExecution.IbOrderID,
                TradeOpened = position.TradeDate,
                TradeClosed = tradeExecution.TradeDate,
                Quantity = tradeQuantity
            });
        }

        private decimal CalculateRealizedPnL(TradeType tradeType, decimal positionAveragePrice, decimal tradeExecutionAveragePrice, decimal tradeQuantity)
        {
            var priceDifference = tradeExecutionAveragePrice - positionAveragePrice;
            if(tradeType == TradeType.Short)
                priceDifference = positionAveragePrice - tradeExecutionAveragePrice; // For short positions, PnL is reversed
            return priceDifference * tradeQuantity;
        }

        private void IncreasePosition(TradeExecution tradeExecution, Position position)
        {
            // Add quantity and recalculate average price
            var totalCost = position.AveragePrice * position.Quantity + tradeExecution.AveragePrice * tradeExecution.Quantity;
            position.Quantity += tradeExecution.Quantity;
            position.AveragePrice = totalCost / position.Quantity;
        }

        private bool AnyOpenPositions(string symbol)
        {
            return positions.Any(x => x.Symbol == symbol && x.IsClosed == false);
        }

        private void OpenPosition(TradeExecution tradeExecution)
        {
            positions.Add(new Position
            {
                AveragePrice = tradeExecution.AveragePrice,
                IbOrderID = tradeExecution.IbOrderID,
                IsClosed = false,
                Quantity = tradeExecution.Quantity,
                Symbol = tradeExecution.Symbol,
                TradeDate = tradeExecution.TradeDate,
                SecurityId = tradeExecution.SecurityId
            });
        }
    }
}
