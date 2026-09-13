using TraderView.Application.Features;

namespace TraderView.Application.Features.EquitySummaries.Query.GetBy
{
    public class GetEquitySummariesByAccountIdSqlQuery : IQueryWithParameters
    {
        private readonly string _accountId;
        public GetEquitySummariesByAccountIdSqlQuery(string accountId) { _accountId = accountId; }

        public Dictionary<string, object> Parameters => new Dictionary<string, object>
        {
            { "@accountId", _accountId }
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
                    WHERE AccountId = @accountId
                    ORDER BY ReportDate DESC";
    }
}
