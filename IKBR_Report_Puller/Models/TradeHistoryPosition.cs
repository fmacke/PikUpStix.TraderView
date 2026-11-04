using DocumentFormat.OpenXml.Drawing.Diagrams;
using TraderView.Domain.Entities;

namespace TraderView.Application.Models
{
    public class TradeHistoryPositions : List<TradeHistoryPosition>
    {
        public TradeHistoryPositions(IReadOnlyList<Position> positions)
        {
            foreach (var pos in positions)
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
        public decimal AverageSalesValue { get; set; }
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
