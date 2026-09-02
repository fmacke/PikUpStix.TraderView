namespace TraderView.Application.Features.CanSlimScreener.Query.GetBy
{
    public class GetLatestScreenerSnapshot : IQueryWithParameters
    {
        public string Script
        {
            get => @"SELECT TOP 1 [Id], [CreatedAt]
                      FROM [TradingBE].[dbo].[CanSlimScreenerSnapshots]
                      ORDER BY [CreatedAt] DESC";
        }

        public Dictionary<string, object> Parameters
        {
            get => new Dictionary<string, object>();
        }
    }
}
