using TraderView.Domain.Backtest;

namespace TraderView.Application.UseCases.Backtest
{
    /// <summary>
    /// Manages pending and active orders/positions during backtesting.
    /// 
    /// Responsibilities:
    /// - Tracks pending entry, stop-loss, and profit-target orders
    /// - Matches orders against bar High/Low (simulates fill)
    /// - Maintains active positions and their realized P&L
    /// - Writes closed trades to a results list
    /// </summary>
    public class TradeBuffer
    {
        /// <summary>
        /// Pending orders waiting to be filled
        /// </summary>
        private readonly List<PendingOrder> _pendingOrders = new();

        /// <summary>
        /// Closed trades (for final results)
        /// </summary>
        public List<BacktestTrade> ClosedTrades { get; } = new();

        /// <summary>
        /// Active open positions keyed by position ID
        /// </summary>
        private readonly Dictionary<int, ActivePosition> _activePositions = new();

        /// <summary>
        /// Next position ID to assign
        /// </summary>
        private int _nextPositionId = 1;

        /// <summary>
        /// Add pending orders (entry, stop, target) to the buffer.
        /// </summary>
        public void PlaceOrders(List<OrderIntent> orders, int barIndex)
        {
            foreach (var order in orders)
            {
                _pendingOrders.Add(new PendingOrder
                {
                    Order = order,
                    PlacedAtBarIndex = barIndex,
                });
            }
        }

        /// <summary>
        /// Attempt to fill pending orders against the current bar's High/Low.
        /// Stops and targets are checked against active positions.
        /// Returns list of newly filled trades (for open or close).
        /// </summary>
        public List<(BacktestTrade?, ActivePosition?)> ProcessBar(
            double barOpen,
            double barHigh,
            double barLow,
            double barClose,
            int barIndex,
            Func<double, double, double> generateFillPrice)
        {
            var results = new List<(BacktestTrade?, ActivePosition?)>();

            // Check stops and targets first (Step A: fill pending orders)
            CheckStopsAndTargets(barOpen, barHigh, barLow, barIndex, results);

            // Then check entry orders (if not already filled a stop/target that closed position)
            FillEntryOrders(barOpen, barHigh, barLow, barIndex, generateFillPrice, results);

            return results;
        }

        /// <summary>
        /// Return all currently open positions
        /// </summary>
        public IEnumerable<ActivePosition> GetOpenPositions() => _activePositions.Values;

        /// <summary>
        /// Check all active stop-losses and profit-targets against bar High/Low.
        /// A stop-loss or target that triggers closes the position.
        /// </summary>
        private void CheckStopsAndTargets(
            double barOpen,
            double barHigh,
            double barLow,
            int barIndex,
            List<(BacktestTrade?, ActivePosition?)> results)
        {
            var ordersToRemove = new List<PendingOrder>();

            // Process stops and targets for each active position
            foreach (var posId in _activePositions.Keys.ToList())
            {
                var position = _activePositions[posId];
                var relevantOrders = _pendingOrders
                    .Where(o => (o.Order.OrderType == OrderType.StopLoss || o.Order.OrderType == OrderType.ProfitTarget) &&
                                o.Order.RelatedPositionId == posId)
                    .ToList();

                foreach (var pendingOrder in relevantOrders)
                {
                    double triggerPrice = pendingOrder.Order.TriggerPrice;
                    bool filled = false;
                    double fillPrice = 0;

                    // Check if bar's High/Low would trigger this order
                    if (position.Direction == PositionDirection.Long)
                    {
                        // Stop-loss below entry, target above entry
                        if (pendingOrder.Order.OrderType == OrderType.StopLoss && barLow <= triggerPrice)
                        {
                            fillPrice = Math.Min(triggerPrice, barOpen); // Conservative: fill at worse price
                            filled = true;
                        }
                        else if (pendingOrder.Order.OrderType == OrderType.ProfitTarget && barHigh >= triggerPrice)
                        {
                            fillPrice = Math.Max(triggerPrice, barOpen);
                            filled = true;
                        }
                    }
                    else if (position.Direction == PositionDirection.Short)
                    {
                        // Stop-loss above entry, target below entry
                        if (pendingOrder.Order.OrderType == OrderType.StopLoss && barHigh >= triggerPrice)
                        {
                            fillPrice = Math.Max(triggerPrice, barOpen);
                            filled = true;
                        }
                        else if (pendingOrder.Order.OrderType == OrderType.ProfitTarget && barLow <= triggerPrice)
                        {
                            fillPrice = Math.Min(triggerPrice, barOpen);
                            filled = true;
                        }
                    }

                    if (filled)
                    {
                        // Close the position
                        var closedTrade = position.Close(
                            exitPrice: fillPrice,
                            exitBarIndex: barIndex,
                            exitType: pendingOrder.Order.OrderType == OrderType.StopLoss
                                ? ExitType.StopLoss
                                : ExitType.ProfitTarget);

                        ClosedTrades.Add(closedTrade);
                        _activePositions.Remove(posId);

                        // Remove all orders related to this position
                        var relatedOrders = _pendingOrders
                            .Where(o => o.Order.RelatedPositionId == posId)
                            .ToList();
                        foreach (var relOrder in relatedOrders)
                        {
                            ordersToRemove.Add(relOrder);
                        }

                        results.Add((closedTrade, null));
                        break; // Only one exit per position per bar
                    }
                }
            }

            foreach (var order in ordersToRemove)
            {
                _pendingOrders.Remove(order);
            }
        }

        /// <summary>
        /// Fill pending entry orders against bar High/Low.
        /// </summary>
        private void FillEntryOrders(
            double barOpen,
            double barHigh,
            double barLow,
            int barIndex,
            Func<double, double, double> generateFillPrice,
            List<(BacktestTrade?, ActivePosition?)> results)
        {
            var ordersToRemove = new List<PendingOrder>();

            foreach (var pendingOrder in _pendingOrders.Where(o => o.Order.OrderType == OrderType.Entry).ToList())
            {
                double triggerPrice = pendingOrder.Order.TriggerPrice;
                bool filled = false;
                double fillPrice = 0;

                // Simplified: assume entry triggers at next bar's open within High/Low range
                if (triggerPrice >= barLow && triggerPrice <= barHigh)
                {
                    fillPrice = generateFillPrice(barOpen, triggerPrice);
                    filled = true;
                }

                if (filled)
                {
                    // Create new position
                    int posId = _nextPositionId++;
                    var activePos = new ActivePosition
                    {
                        PositionId = posId,
                        EntryBarIndex = barIndex,
                        EntryPrice = fillPrice,
                        Direction = pendingOrder.Order.Direction,
                        Quantity = pendingOrder.Order.PositionSize,
                    };

                    _activePositions[posId] = activePos;
                    ordersToRemove.Add(pendingOrder);

                    // Update related stop/target orders with this position's ID
                    var relatedOrders = _pendingOrders
                        .Where(o => o.PlacedAtBarIndex == pendingOrder.PlacedAtBarIndex &&
                                    (o.Order.OrderType == OrderType.StopLoss || o.Order.OrderType == OrderType.ProfitTarget))
                        .ToList();

                    foreach (var relOrder in relatedOrders)
                    {
                        relOrder.Order.RelatedPositionId = posId;
                    }

                    results.Add((null, activePos));
                }
            }

            foreach (var order in ordersToRemove)
            {
                _pendingOrders.Remove(order);
            }
        }

        /// <summary>
        /// Mark-to-market all open positions at given price.
        /// </summary>
        public double GetPortfolioEquity(double currentPrice)
        {
            double totalMtm = 0;
            foreach (var position in _activePositions.Values)
            {
                totalMtm += position.GetUnrealizedPnL(currentPrice);
            }
            return totalMtm;
        }

        /// <summary>
        /// Liquidate all remaining open positions (end of backtest).
        /// </summary>
        public void LiquidateAll(double liquidationPrice, int barIndex)
        {
            foreach (var posId in _activePositions.Keys.ToList())
            {
                var position = _activePositions[posId];
                var trade = position.Close(liquidationPrice, barIndex, ExitType.Liquidation);
                ClosedTrades.Add(trade);
            }
            _activePositions.Clear();
        }
    }

    /// <summary>
    /// Represents a pending order waiting to be filled.
    /// </summary>
    internal class PendingOrder
    {
        public OrderIntent Order { get; set; } = null!;
        public int PlacedAtBarIndex { get; set; }
    }

    /// <summary>
    /// Represents an open position.
    /// </summary>
    public class ActivePosition
    {
        public int PositionId { get; set; }
        public int EntryBarIndex { get; set; }
        public double EntryPrice { get; set; }
        public PositionDirection Direction { get; set; }
        public double Quantity { get; set; }

        /// <summary>
        /// Current unrealized P&L in dollars
        /// </summary>
        public double GetUnrealizedPnL(double currentPrice)
        {
            double priceMove = currentPrice - EntryPrice;
            if (Direction == PositionDirection.Short)
                priceMove *= -1;

            return priceMove * Quantity;
        }

        /// <summary>
        /// Convert to a closed trade
        /// </summary>
        public BacktestTrade Close(double exitPrice, int exitBarIndex, ExitType exitType)
        {
            double pnl = (exitPrice - EntryPrice) * Quantity;
            if (Direction == PositionDirection.Short)
                pnl *= -1;

            return new BacktestTrade
            {
                EntryBarIndex = EntryBarIndex,
                EntryPrice = EntryPrice,
                Direction = Direction,
                Quantity = Quantity,
                ExitBarIndex = exitBarIndex,
                ExitPrice = exitPrice,
                ExitType = exitType,
                GrossProfit = pnl,
                Commission = 0, // Will be set by ExecutionSimulator
                NetProfit = pnl,
                ReturnPercent = (pnl / (EntryPrice * Quantity)) * 100,
            };
        }
    }
}
