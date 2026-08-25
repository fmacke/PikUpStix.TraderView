namespace TraderView.Application.Features.CanSlimScreener.Command
{
    public class CreateCanSlimCandidateCommand
    {
        public string Script()
        {
            return @"
                INSERT INTO [dbo].[CanSlimCandidates]
                   ([CanSlimScreenerSnapshotId]
                   ,[Symbol]
                   ,[Exchange]
                   ,[CompanyName]
                   ,[Sector]
                   ,[Industry]
                   ,[Price]
                   ,[Volume]
                   ,[MarketCap]
                   ,[EvaluationDateUtc]
                   ,[PassesBoth]
                   ,[CurrentQuarter_LatestQuarterDate]
                   ,[CurrentQuarter_LatestQuarterEps]
                   ,[CurrentQuarter_PriorYearQuarterEps]
                   ,[CurrentQuarter_EpsGrowthYoYPercent]
                   ,[CurrentQuarter_RevenueGrowthYoYPercent]
                   ,[CurrentQuarter_IsAccelerating]
                   ,[CurrentQuarter_PassesCriteria]
                   ,[Annual_EpsCagr3YearPercent]
                   ,[Annual_EpsCagr5YearPercent]
                   ,[Annual_ReturnOnEquityPercent]
                   ,[Annual_HasConsecutiveAnnualGrowth]
                   ,[Annual_LatestFiscalYear]
                   ,[Annual_LatestFiscalYearEps]
                   ,[Annual_PriorYear1Eps]
                   ,[Annual_PriorYear2Eps]
                   ,[Annual_PriorYear3Eps]
                   ,[Annual_OperatingMarginPercent]
                   ,[Annual_ReturnOnAssetsPercent]
                   ,[Annual_FundamentalGrade]
                   ,[Annual_PassesCriteria]
                   ,[CreatedAtUtc])
             OUTPUT INSERTED.Id
             VALUES
                   (@CanSlimScreenerSnapshotId
                   ,@Symbol
                   ,@Exchange
                   ,@CompanyName
                   ,@Sector
                   ,@Industry
                   ,@Price
                   ,@Volume
                   ,@MarketCap
                   ,@EvaluationDateUtc
                   ,@PassesBoth
                   ,@CurrentQuarter_LatestQuarterDate
                   ,@CurrentQuarter_LatestQuarterEps
                   ,@CurrentQuarter_PriorYearQuarterEps
                   ,@CurrentQuarter_EpsGrowthYoYPercent
                   ,@CurrentQuarter_RevenueGrowthYoYPercent
                   ,@CurrentQuarter_IsAccelerating
                   ,@CurrentQuarter_PassesCriteria
                   ,@Annual_EpsCagr3YearPercent
                   ,@Annual_EpsCagr5YearPercent
                   ,@Annual_ReturnOnEquityPercent
                   ,@Annual_HasConsecutiveAnnualGrowth
                   ,@Annual_LatestFiscalYear
                   ,@Annual_LatestFiscalYearEps
                   ,@Annual_PriorYear1Eps
                   ,@Annual_PriorYear2Eps
                   ,@Annual_PriorYear3Eps
                   ,@Annual_OperatingMarginPercent
                   ,@Annual_ReturnOnAssetsPercent
                   ,@Annual_FundamentalGrade
                   ,@Annual_PassesCriteria
                   ,@CreatedAtUtc)";
        }
    }
    public class CreateCanSlimScreentSnapshotCommand
    {
        public string Script()
        {
            return @"
                INSERT INTO [dbo].[CanSlimScreenerSnapshots]
                           ([CreatedAt])
                     OUTPUT INSERTED.Id
                     VALUES
                           (@CreatedAt)";
        }
    }
    public class CreateCanSlimCandidateAnnualHistoryCommand
    {
        public string Script()
        {
            return @"
                INSERT INTO [dbo].[CanSlimCandidateAnnualHistory]
                       ([CandidateId]
                       ,[CalendarYear]
                       ,[FiscalDate]
                       ,[Revenue]
                       ,[NetIncome]
                       ,[EpsDiluted]
                       ,[EpsGrowthYoYPercent])
                 OUTPUT INSERTED.Id
                 VALUES
                       (@CandidateId
                       ,@CalendarYear
                       ,@FiscalDate
                       ,@Revenue
                       ,@NetIncome
                       ,@EpsDiluted
                       ,@EpsGrowthYoYPercent)";
        }
    }
}
