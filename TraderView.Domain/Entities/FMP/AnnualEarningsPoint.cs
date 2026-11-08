namespace TraderView.Domain.Entities.FMP
{
    /// <summary>
    /// Represents a single fiscal year's reported financial metrics.
    /// </summary>
    public class AnnualEarningsPoint
    {
        public int CandidateId { get; set; }
        public string CalendarYear { get; set; } = string.Empty;
        public string FiscalDate { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal NetIncome { get; set; }
        public decimal EpsDiluted { get; set; }
        public decimal EpsGrowthYoYPercent { get; set; }
    }
}
