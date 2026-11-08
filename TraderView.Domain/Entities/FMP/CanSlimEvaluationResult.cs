namespace TraderView.Domain.Entities.FMP
{
    public class CanSlimEvaluationResult
    {
        public string Symbol { get; set; } = string.Empty;
        public CanSlimCurrentQuarterMetric? CurrentQuarter { get; set; }
        public CanSlimAnnualMetric? Annual { get; set; }
        public bool PassesBoth { get; set; }
    }
}
