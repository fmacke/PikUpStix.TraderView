namespace TraderView.Application.Features.CanSlimScreener.Query.GetBy
{
    public class GetLatestScreenerSnapshot
    {
        public string Script()
        {
            return @"SELECT TOP 1 [Id], [CreatedAt]
                      FROM [TradingBE].[dbo].[CanSlimScreenerSnapshots]
                      ORDER BY [CreatedAt] DESC";
        }
    }
}
