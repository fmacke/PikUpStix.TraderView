using DocumentFormat.OpenXml.Drawing.Diagrams;
using TraderView.Domain.Entities;

namespace TraderView.Application.Models
{
    public class TradeHistoryReport : List<TradeHistoryPosition>
    {
        public decimal WinRatePercentage { get { return this.Count > 0 ? this.Count(x => x.NetMargin > 0) / Convert.ToDecimal(this.Count) : 0; } }
        public decimal LossRatePercentage { get { return 1m - WinRatePercentage; } }
        public decimal GainPercentage { get { return this.Count > 0 ? (this.Where(x => x.NetMargin > 0).Sum(x => x.NetMargin)  / this.Sum(x => x.TotalPurchaseCost)) : 0; } }
        public decimal LossPercentage { get { return this.Count > 0 ? (this.Where(x => x.NetMargin < 0).Sum(x => x.NetMargin) / this.Sum(x => x.TotalPurchaseCost)) : 0; } }
        public decimal RewardToRiskRatio { get { return LossPercentage == 0 ? 0 : GainPercentage / Math.Abs(LossPercentage); } }
        public int NumberOfTrades { get { return this.Count; } }
        public decimal SimpleRoi { get { decimal winRate = WinRatePercentage / 100m; decimal lossRate = 1m - winRate; return (NumberOfTrades * ((winRate) * (GainPercentage / 100m) - (lossRate) * (Math.Abs(LossPercentage) / 100m))) * 100m; } }
        public decimal CompoundedRoi { get { decimal winRate = WinRatePercentage / 100m; decimal lossRate = 1m - winRate; return ((decimal)Math.Pow((double)(1.0m + (GainPercentage / 100m)), (double)(NumberOfTrades * winRate)) * (decimal)Math.Pow((double)(1.0m - (Math.Abs(LossPercentage) / 100m)), (double)(NumberOfTrades * lossRate)) - 1.0m); } }
        public decimal ExpectedReturnPerTrade { get { return NumberOfTrades > 0 ? SimpleRoi / NumberOfTrades : 0; } }
        public decimal AverageDaysHeld { get { return this.Count > 0 ? this.Average(x => (decimal)x.DaysHeld) : 0; } }
        public decimal AverageDaysHeldGains { get { return this.Count > 0 ? this.Where(x => x.NetMargin > 0).Average(x => (decimal)x.DaysHeld) : 0; } }
        public decimal AverageDaysHeldLosses { get { return this.Count > 0 ? this.Where(x => x.NetMargin < 0).Average(x => (decimal)x.DaysHeld) : 0; } }
        public decimal AverageWin { get { return this.Count > 0 ? this.Where(x => x.NetMargin > 0).Average(x => x.NetMargin) : 0; } }
        public decimal AverageLoss { get { return this.Count > 0 ? this.Where(x => x.NetMargin < 0).Average(x => x.NetMargin) : 0; } }
        public decimal LargestWin { get { return this.Count > 0 ? this.Where(x => x.NetMargin > 0).Max(x => x.NetMargin) : 0; } }
        public int LargestWinPositionId { get { return this.Count > 0 ? this.Where(x => x.NetMargin > 0).OrderByDescending(x => x.NetMargin).First().Id : 0; } }
        public decimal LargestLoss { get { return this.Count > 0 ? this.Where(x => x.NetMargin < 0).Min(x => x.NetMargin) : 0; } }
        public int LargestLossPositionId { get { return this.Count > 0 ? this.Where(x => x.NetMargin < 0).OrderBy(x => x.NetMargin).First().Id : 0; } }

        public TradeHistoryReport(IReadOnlyList<Position> positions)
        {
            foreach (var pos in positions.Where(x => x.Status.ToLower() == "closed"))
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
                    CommissionCurrency = pos.TradeExecutions.FirstOrDefault().IbCommissionCurrency,
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
                return Convert.ToDecimal(tradeExecutions.Where(x => x.BuySell == direction).Sum(x => x.TradePrice * x.Quantity) / quantity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating cost basis for direction {direction} on trade executions for {string.Join(", ", tradeExecutions.Select(te => te.Id))}: {ex.Message}");
                return 0; // or throw, depending on how you want to handle this
            }
        }
        private decimal CalculateSalesValue(string direction, ICollection<TradeExecution> tradeExecutions)
        {
            try
            {
                var quantity = CalculateTotalQuantity(direction, tradeExecutions) * -1;
                return Convert.ToDecimal(tradeExecutions.Where(x => x.BuySell != direction).Sum(x => x.TradePrice * x.Quantity) / quantity);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating sales value for direction {direction} on trade executions for {string.Join(", ", tradeExecutions.Select(te => te.Id))}: {ex.Message}");
                return 0; // or throw, depending on how you want to handle this
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
                Console.WriteLine($"Error calculating total quantity for direction {direction} on trade executions for {string.Join(", ", tradeExecutions.Select(te => te.Id))}: {ex.Message}");
                return 0; // or throw, depending on how you want to handle this
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
        public double DaysHeld { get { return (CloseDate - OpenDate).TotalDays; } }
        public decimal AverageCostBasis { get; set; }
        public int InstrumentId { get; set; }
        public decimal AverageSalesValue { get; set; }
        public string MarketRegime { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalPurchaseCost { get { return AverageCostBasis * Quantity; } }
        public decimal TotalSalesValue { get { return AverageSalesValue * Quantity; } }
        public decimal IbCommission { get; set; }
        public decimal GrossMargin { get { return TotalSalesValue - TotalPurchaseCost; } }
        public decimal NetMargin { get { return GrossMargin + IbCommission; } }
        public decimal MaximumFavourableExcursion { get; set; }
        public decimal MaximumAdverseExcursion { get; set; }
        public string Direction { get; set; }
        public decimal NetMarginPercent { get {
                if (Direction == "SELL")
                    return TotalPurchaseCost != 0 ? NetMargin / TotalPurchaseCost * -1 : 0;
                return TotalPurchaseCost != 0 ? NetMargin / TotalPurchaseCost : 0;
            } }
    }
}
