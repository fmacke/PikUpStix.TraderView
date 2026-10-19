using System;

namespace traderview.Server.DTOs
{
    public class PositionOutcomeDto
    {
        public int Id { get; set; }
        public DateTime? OpenDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public string? Status { get; set; }
        public int InstrumentId { get; set; }
        public decimal? LastReportedPrice { get; set; }
        public DateTime? LastReportedPriceUpdated { get; set; }
        public string? MarketRegime { get; set; }
        public decimal MaximumFavourableExcursion { get; set; }
        public decimal MaximumAdverseExcursion { get; set; }
    }
}
