using TraderView.Application.Features;

namespace TraderView.Application.Features.EquitySummaries.Query.GetBy
{
    public class GetEquitySummaryByAccountAndDateSqlQuery : IQueryWithParameters
    {
        private readonly string _accountId;
        private readonly DateTime _reportDate;

        public GetEquitySummaryByAccountAndDateSqlQuery(string accountId, DateTime reportDate)
        {
            _accountId = accountId;
            _reportDate = reportDate.Date;
        }

        public Dictionary<string, object> Parameters => new Dictionary<string, object>
        {
            { "@accountId", _accountId },
            { "@reportDate", _reportDate }
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
                    AND ReportDate = @reportDate";
    }
}
