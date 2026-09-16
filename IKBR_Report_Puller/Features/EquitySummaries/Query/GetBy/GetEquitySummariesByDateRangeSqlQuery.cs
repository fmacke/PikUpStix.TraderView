using TraderView.Application.Features;

namespace TraderView.Application.Features.EquitySummaries.Query.GetBy
{
    public class GetEquitySummariesByDateRangeSqlQuery : IQueryWithParameters
    {
        private readonly DateTime _startDate;
        private readonly DateTime _endDate;

        public GetEquitySummariesByDateRangeSqlQuery(DateTime startDate, DateTime endDate)
        {
            _startDate = startDate.Date;
            _endDate = endDate.Date;
        }

        public Dictionary<string, object> Parameters => new Dictionary<string, object>
        {
            { "@startDate", _startDate },
            { "@endDate", _endDate }
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
                    WHERE ReportDate >= @startDate
                    AND ReportDate <= @endDate
                    ORDER BY ReportDate DESC, AccountId ASC";
    }
}
