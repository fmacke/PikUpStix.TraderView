using System;

namespace TraderView.Domain.Entities
{
    public class Position
    {
        public int Id { get; set; }
        public DateTime OpenDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public string Status { get; set; } = null!;
        public int InstrumentId { get; set; }
        public decimal? LastReportedPrice { get; set; }
        public DateTime? LastReportedPriceUpdated { get; set; }
        public decimal? GrossProfit { get; set; }
        public decimal? Commission { get; set; }
        public decimal? NetProfit { get; set; }
        public decimal? MaximumFavourableExcursion { get; set; }
        public decimal? MaximumAdverseExcursion { get; set; }
        public string? Direction { get; set; }
        public string? MarketRegime { get; set; }
        public virtual Instrument Instrument { get; set; } = null!;
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
        public virtual ICollection<TradeExecution> TradeExecutions { get; set; } = new List<TradeExecution>();
    }
}
