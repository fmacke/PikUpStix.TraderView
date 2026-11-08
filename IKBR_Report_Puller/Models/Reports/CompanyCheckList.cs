namespace TraderView.Application.Models.Reports
{
    public class CompanyChecklistReport
    {
        public string Symbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;

        // Market & Industry Performance
        public string CompositeRating { get; set; } = "N/A"; // e.g. "93" or "A+"
        public string MarketExposureStatus { get; set; } = "N/A";
        public string IndustryGroup { get; set; } = "N/A";
        public int? IndustryGroupRank { get; set; }
        public string GroupRsRating { get; set; } = "N/A";

        // Fundamental - Current Earnings
        public DateTime? EpsDueDate { get; set; }
        public int EpsRating { get; set; }
        public decimal LastQtrEpsGrowthYoY { get; set; }
        public decimal Last3QtrsAvgEpsGrowth { get; set; }
        public int QtrsEpsAccelerationCount { get; set; }
        public decimal? CurrentQtrEpsEstChg { get; set; }
        public string EstimateRevisionsTrend { get; set; } = "Flat"; // Up, Down, Flat
        public decimal? LastQtrEarningsSurprisePercent { get; set; }

        // Fundamental - Annual Earnings
        public decimal Eps3YrCagr { get; set; }
        public int ConsecutiveYrsAnnualEpsGrowth { get; set; }
        public decimal? CurrentYearEpsEstChg { get; set; }

        // Fundamental - Sales, Margin, ROE
        public string SmrRating { get; set; } = "N/A"; // A - E
        public decimal LastQtrSalesGrowthYoY { get; set; }
        public decimal Sales3YrCagr { get; set; }
        public decimal AnnualPreTaxMarginPercent { get; set; }
        public decimal AnnualRoePercent { get; set; }
        public decimal DebtToEquityRatio { get; set; }

        // Technical Performance - Price & Volume
        public decimal Price { get; set; }
        public int RelativeStrengthRating { get; set; } // 1 - 99
        public decimal PercentOff52WeekHigh { get; set; }
        public decimal PriceVs50DaySmaPercent { get; set; }
        public long Volume50DayAverage { get; set; }

        // Supply and Demand
        public decimal MarketCapBillions { get; set; }
        public string AccumulationDistributionRating { get; set; } = "C"; // A - E
        public decimal UpDownVolumeRatio { get; set; }
        public decimal? FundOwnershipChangePercent { get; set; }
        public int? QtrsIncreasingFundOwnership { get; set; }
    }
}