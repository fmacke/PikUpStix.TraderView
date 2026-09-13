using TraderView.Application.Features;

namespace TraderView.Application.Features.EquitySummaries.Query.Get
{
    public class GetAllEquitySummariesSqlQuery : IQueryWithParameters
    {
        public Dictionary<string, object> Parameters => new Dictionary<string, object>();

        public string Script => @"SELECT 
                        Id, AccountId, AcctAlias, Model, Currency, ReportDate,
                        Cash, CashLong, CashShort,
                        Stock, StockLong, StockShort,
                        Funds, FundsLong, FundsShort,
                        DividendAccruals, DividendAccrualsLong, DividendAccrualsShort,
                        Total, TotalLong, TotalShort,
                        CreatedAt
                    FROM [dbo].[EquitySummaries]
                    ORDER BY ReportDate DESC, AccountId ASC";
    }
}
