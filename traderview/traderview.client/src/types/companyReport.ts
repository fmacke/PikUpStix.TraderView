export interface CompanyChecklistReportDto {
    symbol: string;
    companyName: string;
    generatedAtUtc: string;

    // Market & Industry Performance
    compositeRating: string;
    marketExposureStatus: string;
    industryGroup: string;
    industryGroupRank: number | null;
    groupRsRating: string;

    // Fundamental - Current Earnings
    epsDueDate: string | null;
    epsRating: number;
    lastQtrEpsGrowthYoY: number;
    last3QtrsAvgEpsGrowth: number;
    qtrsEpsAccelerationCount: number;
    currentQtrEpsEstChg: number | null;
    estimateRevisionsTrend: string;
    lastQtrEarningsSurprisePercent: number | null;

    // Fundamental - Annual Earnings
    eps3YrCagr: number;
    consecutiveYrsAnnualEpsGrowth: number;
    currentYearEpsEstChg: number | null;

    // Fundamental - Sales, Margin, ROE
    smrRating: string;
    lastQtrSalesGrowthYoY: number;
    sales3YrCagr: number;
    annualPreTaxMarginPercent: number;
    annualRoePercent: number;
    debtToEquityRatio: number;

    // Technical Performance - Price & Volume
    price: number;
    relativeStrengthRating: number;
    percentOff52WeekHigh: number;
    priceVs50DaySmaPercent: number;
    volume50DayAverage: number;

    // Supply and Demand
    marketCapBillions: number;
    accumulationDistributionRating: string;
    upDownVolumeRatio: number;
    fundOwnershipChangePercent: number | null;
    qtrsIncreasingFundOwnership: number | null;
}

export interface RatingIndicator {
    value: string | number;
    rating: 'green' | 'red' | 'yellow' | 'neutral';
}
