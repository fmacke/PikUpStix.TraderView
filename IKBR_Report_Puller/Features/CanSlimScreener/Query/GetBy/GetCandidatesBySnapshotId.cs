namespace TraderView.Application.Features.Instruments.Query.GetBy
{
    public class GetCandidatesBySnapshotIdQuery : IQuery<string>
    {
        public string Script()
        {
            return @"SELECT [Id]
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
                      ,[CreatedAtUtc]
                  FROM [TradingBE].[dbo].[CanSlimCandidates]
                  Where [CanSlimScreenerSnapshotId] = @CanSlimScreenerSnapshotId";
        }
    }
}
