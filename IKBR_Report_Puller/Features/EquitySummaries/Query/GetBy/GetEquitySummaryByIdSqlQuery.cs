using TraderView.Application.Features;

namespace TraderView.Application.Features.EquitySummaries.Query.GetBy
{
    public class GetEquitySummaryByIdSqlQuery : IQueryWithParameters
    {
        private readonly int _id;
        public GetEquitySummaryByIdSqlQuery(int id) { _id = id; }

        public Dictionary<string, object> Parameters => new Dictionary<string, object>
        {
            { "@id", _id }
        };

        public string Script => @"SELECT 
                        Id, AccountId, AcctAlias, Model, Currency, ReportDate,
                        Cash, CashLong, CashShort,
                        Stock, StockLong, StockShort,
                        Funds, FundsLong, FundsShort,
                        DividendAccruals, DividendAccrualsLong, DividendAccrualsShort,
                        Total, TotalLong, TotalShort,
                        CreatedAt
                    FROM [dbo].[EquitySummaries]
                    WHERE Id = @id";
    }
}
