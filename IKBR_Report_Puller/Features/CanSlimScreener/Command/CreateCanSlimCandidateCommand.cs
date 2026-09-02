using System.Windows.Input;
using TraderView.Application.Mappers;
using TraderView.Domain.Entities.FMP;
namespace TraderView.Application.Features.CanSlimScreener.Command
{
    public class CreateCanSlimCandidateCommand : IQueryWithParameters
    {
        private readonly CanSlimCandidate candidate;
        public CreateCanSlimCandidateCommand(CanSlimCandidate candidate)
        {
            this.candidate = candidate;
        }
        public Dictionary<string, object> Parameters { get => MapToSql.GetCanSlimCandidate(candidate); }
        public string Script
        {
            get => @"
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
    public class CreateCanSlimScreenSnapshotCommand : IQueryWithParameters
    {
        public Dictionary<string, object> Parameters
        {
            get => new Dictionary<string, object>
            {
                { "@CreatedAt", DateTime.UtcNow }
            };
        }

        public string Script
        {
            get => @"
                INSERT INTO [dbo].[CanSlimScreenerSnapshots]
                           ([CreatedAt])
                     OUTPUT INSERTED.Id
                     VALUES
                           (@CreatedAt)";
        }
    }
    public class CreateCanSlimCandidateAnnualHistoryCommand : IQueryWithParameters
    {
        public CreateCanSlimCandidateAnnualHistoryCommand(CanSlimCandidateAnnualHistory annualHistory)
        {
            Parameters = MapToSql.GetCanSlimAnnualHistory(annualHistory);
        }
        public Dictionary<string, object> Parameters { get; }
        public string Script
        {
            get => @"
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
