namespace TraderView.Domain.Entities.FMP
{
    public class CanSlimCurrentQuarterMetric
    {
        public string Symbol { get; set; } = string.Empty;
        public string LatestQuarterDate { get; set; } = string.Empty;
        public decimal LatestQuarterEps { get; set; }
        public decimal PriorYearQuarterEps { get; set; }
        public decimal EpsGrowthYoYPercent { get; set; }
        public decimal RevenueGrowthYoYPercent { get; set; }
        public bool IsAccelerating { get; set; }
        public bool PassesCriteria { get; set; }
    }
}
