-- SQL Script to create CompanyChecklistReports table
-- Generated for CompanyChecklistReport entity persistence

-- Create CompanyChecklistReports table
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CompanyChecklistReports]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[CompanyChecklistReports] (
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[Symbol] [varchar](16) NOT NULL,
		[CompanyName] [varchar](200) NULL,
		[CreatedAtUtc] [datetime] NOT NULL DEFAULT (getutcdate()),

		-- Market & Industry Performance
		[CompositeRating] [varchar](10) NOT NULL DEFAULT (N'N/A'),
		[MarketExposureStatus] [varchar](50) NOT NULL DEFAULT (N'N/A'),
		[IndustryGroup] [varchar](100) NOT NULL DEFAULT (N'N/A'),
		[IndustryGroupRank] [int] NULL,
		[GroupRsRating] [varchar](10) NOT NULL DEFAULT (N'N/A'),

		-- Fundamental - Current Earnings
		[EpsDueDate] [datetime] NULL,
		[EpsRating] [int] NOT NULL,
		[LastQtrEpsGrowthYoY] [decimal](9, 4) NOT NULL,
		[Last3QtrsAvgEpsGrowth] [decimal](9, 4) NOT NULL,
		[QtrsEpsAccelerationCount] [int] NOT NULL,
		[CurrentQtrEpsEstChg] [decimal](9, 4) NULL,
		[EstimateRevisionsTrend] [varchar](20) NOT NULL DEFAULT (N'Flat'),
		[LastQtrEarningsSurprisePercent] [decimal](9, 4) NULL,

		-- Fundamental - Annual Earnings
		[Eps3YrCagr] [decimal](9, 4) NOT NULL,
		[ConsecutiveYrsAnnualEpsGrowth] [int] NOT NULL,
		[CurrentYearEpsEstChg] [decimal](9, 4) NULL,

		-- Fundamental - Sales, Margin, ROE
		[SmrRating] [varchar](5) NOT NULL DEFAULT (N'N/A'),
		[LastQtrSalesGrowthYoY] [decimal](9, 4) NOT NULL,
		[Sales3YrCagr] [decimal](9, 4) NOT NULL,
		[AnnualPreTaxMarginPercent] [decimal](9, 4) NOT NULL,
		[AnnualRoePercent] [decimal](9, 4) NOT NULL,
		[DebtToEquityRatio] [decimal](9, 4) NOT NULL,

		-- Technical Performance - Price & Volume
		[Price] [decimal](18, 4) NOT NULL,
		[RelativeStrengthRating] [int] NOT NULL,
		[PercentOff52WeekHigh] [decimal](9, 4) NOT NULL,
		[PriceVs50DaySmaPercent] [decimal](9, 4) NOT NULL,
		[Volume50DayAverage] [bigint] NOT NULL,

		-- Supply and Demand
		[MarketCapBillions] [decimal](19, 2) NOT NULL,
		[AccumulationDistributionRating] [varchar](5) NOT NULL DEFAULT (N'C'),
		[UpDownVolumeRatio] [decimal](9, 4) NOT NULL,
		[FundOwnershipChangePercent] [decimal](9, 4) NULL,

		CONSTRAINT [PK_CompanyChecklistReports] PRIMARY KEY CLUSTERED 
		(
			[Id] ASC
		)
	) ON [PRIMARY]
END
GO

-- Create index on Symbol and CreatedAtUtc (descending for recent queries)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CompanyChecklistReport_Symbol_CreatedAt' AND object_id = OBJECT_ID(N'[dbo].[CompanyChecklistReports]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_CompanyChecklistReport_Symbol_CreatedAt] 
	ON [dbo].[CompanyChecklistReports] ([Symbol] ASC, [CreatedAtUtc] DESC)
	INCLUDE ([CompanyName], [Price], [RelativeStrengthRating])
END
GO

-- Create index on Symbol for quick lookups
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CompanyChecklistReport_Symbol' AND object_id = OBJECT_ID(N'[dbo].[CompanyChecklistReports]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_CompanyChecklistReport_Symbol] 
	ON [dbo].[CompanyChecklistReports] ([Symbol] ASC)
END
GO

-- Create index on CreatedAtUtc for date range queries
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CompanyChecklistReport_CreatedAt' AND object_id = OBJECT_ID(N'[dbo].[CompanyChecklistReports]'))
BEGIN
	CREATE NONCLUSTERED INDEX [IX_CompanyChecklistReport_CreatedAt] 
	ON [dbo].[CompanyChecklistReports] ([CreatedAtUtc] DESC)
END
GO
