namespace TraderView.Domain.Entities.FMP
{
    public class CanSlimCandidate
    {
        public int Id { get; set; }

        public int CanSlimScreenerSnapshotId { get; set; }

        public string Symbol { get; set; } = null!;

        public string? Exchange { get; set; }

        public string? CompanyName { get; set; }

        public string? Sector { get; set; }

        public string? Industry { get; set; }

        public decimal Price { get; set; }

        public decimal Volume { get; set; }

        public decimal MarketCap { get; set; }

        public DateTime EvaluationDateUtc { get; set; }

        public bool PassesBoth { get; set; }

        public string? CurrentQuarterLatestQuarterDate { get; set; }

        public decimal CurrentQuarterLatestQuarterEps { get; set; }

        public decimal CurrentQuarterPriorYearQuarterEps { get; set; }

        public decimal CurrentQuarterEpsGrowthYoYpercent { get; set; }

        public decimal CurrentQuarterRevenueGrowthYoYpercent { get; set; }

        public bool CurrentQuarterIsAccelerating { get; set; }

        public bool CurrentQuarterPassesCriteria { get; set; }

        public decimal AnnualEpsCagr3YearPercent { get; set; }

        public decimal? AnnualEpsCagr5YearPercent { get; set; }

        public decimal AnnualReturnOnEquityPercent { get; set; }

        public bool AnnualHasConsecutiveAnnualGrowth { get; set; }

        public string? AnnualLatestFiscalYear { get; set; }

        public decimal AnnualLatestFiscalYearEps { get; set; }

        public decimal AnnualPriorYear1Eps { get; set; }

        public decimal AnnualPriorYear2Eps { get; set; }

        public decimal AnnualPriorYear3Eps { get; set; }

        public decimal AnnualOperatingMarginPercent { get; set; }

        public decimal AnnualReturnOnAssetsPercent { get; set; }

        public string AnnualFundamentalGrade { get; set; } = null!;

        public bool AnnualPassesCriteria { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public virtual ICollection<CanSlimCandidateAnnualHistory> CanSlimCandidateAnnualHistories { get; set; } = new List<CanSlimCandidateAnnualHistory>();

        public virtual CanSlimScreenerSnapshot CanSlimScreenerSnapshot { get; set; } = null!;
    }
    public partial class CanSlimCandidateAnnualHistory
    {
        public int Id { get; set; }

        public int CandidateId { get; set; }

        public string CalendarYear { get; set; } = null!;

        public string? FiscalDate { get; set; }

        public decimal Revenue { get; set; }

        public decimal NetIncome { get; set; }

        public decimal EpsDiluted { get; set; }

        public decimal EpsGrowthYoYpercent { get; set; }

        public virtual CanSlimCandidate Candidate { get; set; } = null!;
    }
}
