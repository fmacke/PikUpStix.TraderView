using System;

namespace PikUpStix.TraderView.Models
{
    /// <summary>
    /// Represents the calculated data for an open position in a report format.
    /// This model contains all the computed values needed for displaying open positions
    /// in various report formats (Excel, Web, etc.)
    /// </summary>
    public class OpenPositionReportData
    {
        public string AccountId { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public DateTime? DateOpened { get; set; }
        public int? DaysOpened { get; set; }
        public decimal Quantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal Value { get; set; }
        public decimal UnrealizedPnL { get; set; }
        public decimal PercentChange { get; set; }
        public decimal CurrentMargin { get; set; }
    }
}
