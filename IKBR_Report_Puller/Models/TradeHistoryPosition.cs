using DocumentFormat.OpenXml.Drawing.Diagrams;
using System;
using System.Collections.Generic;
using System.Linq;
using TraderView.Domain.Entities;

namespace TraderView.Application.Models
{
    public class TradeHistoryReport : List<TradeHistoryPosition>
    {
        // 0 to 100 percentage scale (e.g. 37.67m)
        public decimal WinRatePercentage =>
            this.Count > 0 ? (this.Count(x => x.NetMargin > 0) / (decimal)this.Count) * 100m : 0m;

        public decimal LossRatePercentage => 100m - WinRatePercentage;

        // True mean trade percentage gains/losses (+7.17% / -4.27%)
        public decimal GainPercentage =>
            this.Any(x => x.NetMargin > 0)
                ? this.Where(x => x.NetMargin > 0).Average(x => x.NetMarginPercent) * 100m
                : 0m;

        public decimal LossPercentage =>
            this.Any(x => x.NetMargin < 0)
                ? this.Where(x => x.NetMargin < 0).Average(x => x.NetMarginPercent) * 100m
                : 0m;

        // Payoff Ratio: Avg Dollar Win / Abs(Avg Dollar Loss) -> 1.77 (or Avg Gain % / Abs(Avg Loss %))
        public decimal RewardToRiskRatio =>
            AverageLoss != 0m ? AverageWin / Math.Abs(AverageLoss) : 0m;

        public int NumberOfTrades => this.Count;

        // Total Cumulative Net Margin ($) across all trades
        public decimal TotalNetMargin => this.Sum(x => x.NetMargin);

        // Expectancy per trade in Dollars (e.g. +$13.49 per trade)
        public decimal ExpectedReturnPerTrade =>
            NumberOfTrades > 0 ? TotalNetMargin / NumberOfTrades : 0m;

        // Expectancy per trade in Percentage (e.g. +0.04% per trade)
        public decimal ExpectedReturnPerTradePercent =>
            NumberOfTrades > 0
                ? ((WinRatePercentage / 100m) * (GainPercentage / 100m)) +
                  ((LossRatePercentage / 100m) * (LossPercentage / 100m))
                : 0m;

        // Simple Cumulative Return (%): Sum of all individual NetMarginPercent returns
        public decimal SimpleRoi => this.Sum(x => x.NetMarginPercent) * 100m;

        public decimal CompoundedRoi
        {
            get
            {
                if (this.Count == 0) return 0m;

                // Start with a normalized base equity of 1.0 (or initial capital)
                decimal runningEquity = 1.0m;

                // Order chronologically by CloseDate to ensure correct chronological sequence compounding
                var chronologicalTrades = this.OrderBy(x => x.CloseDate).ThenBy(x => x.OpenDate);

                foreach (var pos in chronologicalTrades)
                {
                    // Compound each trade's fractional return sequentially
                    // Ensure NetMarginPercent is represented as a decimal fraction (e.g. 0.05 for +5%)
                    decimal tradeReturnFraction = pos.NetMarginPercent;

                    runningEquity *= (1.0m + tradeReturnFraction);
                }

                return (runningEquity - 1.0m) * 100m;
            }
        }

        public decimal AverageDaysHeld =>
            this.Count > 0 ? (decimal)this.Average(x => x.DaysHeld) : 0m;

        public decimal AverageDaysHeldGains =>
            this.Any(x => x.NetMargin > 0)
                ? (decimal)this.Where(x => x.NetMargin > 0).Average(x => x.DaysHeld)
                : 0m;

        public decimal AverageDaysHeldLosses =>
            this.Any(x => x.NetMargin < 0)
                ? (decimal)this.Where(x => x.NetMargin < 0).Average(x => x.DaysHeld)
                : 0m;

        public decimal AverageWin =>
            this.Any(x => x.NetMargin > 0) ? this.Where(x => x.NetMargin > 0).Average(x => x.NetMargin) : 0m;

        public decimal AverageLoss =>
            this.Any(x => x.NetMargin < 0) ? this.Where(x => x.NetMargin < 0).Average(x => x.NetMargin) : 0m;

        public decimal LargestWin =>
            this.Any(x => x.NetMargin > 0) ? this.Where(x => x.NetMargin > 0).Max(x => x.NetMargin) : 0m;

        public int LargestWinPositionId =>
            this.Any(x => x.NetMargin > 0) ? this.Where(x => x.NetMargin > 0).OrderByDescending(x => x.NetMargin).First().Id : 0;

        public decimal LargestLoss =>
            this.Any(x => x.NetMargin < 0) ? this.Where(x => x.NetMargin < 0).Min(x => x.NetMargin) : 0m;

        public int LargestLossPositionId =>
            this.Any(x => x.NetMargin < 0) ? this.Where(x => x.NetMargin < 0).OrderBy(x => x.NetMargin).First().Id : 0;

        public TradeHistoryReport(IReadOnlyList<Position> positions)
        {
            foreach (var pos in positions.Where(x => x.Status.Equals("closed", StringComparison.OrdinalIgnoreCase)))
            {
                var tradeHistoryPosition = new TradeHistoryPosition
                {
                    Id = pos.Id,
                    Symbol = pos.Instrument.DataName,
                    OpenDate = pos.OpenDate,
                    CloseDate = Convert.ToDateTime(pos.CloseDate),
                    AverageCostBasis = CalculateCostBasis(pos.Direction, pos.TradeExecutions),
                    AverageSalesValue = CalculateSalesValue(pos.Direction, pos.TradeExecutions),
                    Quantity = CalculateTotalQuantity(pos.Direction, pos.TradeExecutions),
                    IbCommission = Convert.ToDecimal(pos.TradeExecutions.Sum(x => x.IbCommission)),
                    MaximumFavourableExcursion = Convert.ToDecimal(pos.MaximumFavourableExcursion),
                    MaximumAdverseExcursion = Convert.ToDecimal(pos.MaximumAdverseExcursion),
                    Direction = pos.Direction,
                    Currency = pos.Instrument.Currency,
                    CommissionCurrency = pos.TradeExecutions.FirstOrDefault()?.IbCommissionCurrency ?? "USD",
                    InstrumentId = pos.InstrumentId,
                    MarketRegime = pos.MarketRegime
                };
                this.Add(tradeHistoryPosition);
            }
        }

        private decimal CalculateCostBasis(string direction, ICollection<TradeExecution> tradeExecutions)
        {
            try
            {
                var quantity = CalculateTotalQuantity(direction, tradeExecutions);
                if (quantity == 0m) return 0m;
                return Convert.ToDecimal(tradeExecutions.Where(x => x.BuySell == direction).Sum(x => x.TradePrice * x.Quantity) / quantity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating cost basis for direction {direction}: {ex.Message}");
                return 0m;
            }
        }

        private decimal CalculateSalesValue(string direction, ICollection<TradeExecution> tradeExecutions)
        {
            try
            {
                var quantity = CalculateTotalQuantity(direction, tradeExecutions) * -1m;
                if (quantity == 0m) return 0m;
                return Convert.ToDecimal(tradeExecutions.Where(x => x.BuySell != direction).Sum(x => x.TradePrice * x.Quantity) / quantity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating sales value for direction {direction}: {ex.Message}");
                return 0m;
            }
        }

        private decimal CalculateTotalQuantity(string direction, ICollection<TradeExecution> tradeExecutions)
        {
            try
            {
                return Convert.ToDecimal(tradeExecutions.Where(x => x.BuySell == direction).Sum(x => x.Quantity));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating total quantity for direction {direction}: {ex.Message}");
                return 0m;
            }
        }
    }

    public class TradeHistoryPosition
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public string Currency { get; set; }
        public string CommissionCurrency { get; set; }
        public DateTime OpenDate { get; set; }
        public DateTime CloseDate { get; set; }
        public double DaysHeld => (CloseDate - OpenDate).TotalDays;
        public decimal AverageCostBasis { get; set; }
        public int InstrumentId { get; set; }
        public decimal AverageSalesValue { get; set; }
        public string MarketRegime { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalPurchaseCost => AverageCostBasis * Quantity;
        public decimal TotalSalesValue => AverageSalesValue * Quantity;
        public decimal IbCommission { get; set; }
        public decimal GrossMargin => TotalSalesValue - TotalPurchaseCost;
        public decimal NetMargin => GrossMargin + IbCommission;
        public decimal MaximumFavourableExcursion { get; set; }
        public decimal MaximumAdverseExcursion { get; set; }
        public string Direction { get; set; }

        public decimal NetMarginPercent
        {
            get
            {
                if (TotalPurchaseCost == 0m) return 0m;
                var pct = NetMargin / Math.Abs(TotalPurchaseCost);
                return Direction.Equals("SELL", StringComparison.OrdinalIgnoreCase) ? pct * -1m : pct;
            }
        }
    }
}