using TraderView.Application.Features;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.EquitySummaries.Command.Create
{
    public class InsertEquitySummarySqlCommand : IQueryWithParameters
    {
        private readonly EquitySummary _entity;
        public InsertEquitySummarySqlCommand(EquitySummary entity)
        {
            _entity = entity;
        }

        public Dictionary<string, object> Parameters => new Dictionary<string, object>
        {
            { "@accountId", _entity.AccountId },
            { "@acctAlias", _entity.AcctAlias ?? (object)DBNull.Value },
            { "@model", _entity.Model ?? (object)DBNull.Value },
            { "@currency", _entity.Currency },
            { "@reportDate", _entity.ReportDate.Date },
            { "@cash", _entity.Cash },
            { "@cashLong", _entity.CashLong },
            { "@cashShort", _entity.CashShort },
            { "@stock", _entity.Stock },
            { "@stockLong", _entity.StockLong },
            { "@stockShort", _entity.StockShort },
            { "@funds", _entity.Funds },
            { "@fundsLong", _entity.FundsLong },
            { "@fundsShort", _entity.FundsShort },
            { "@dividendAccruals", _entity.DividendAccruals },
            { "@dividendAccrualsLong", _entity.DividendAccrualsLong },
            { "@dividendAccrualsShort", _entity.DividendAccrualsShort },
            { "@total", _entity.Total },
            { "@totalLong", _entity.TotalLong },
            { "@totalShort", _entity.TotalShort }
        };

        public string Script => @"INSERT INTO [dbo].[EquitySummaries]
                    (AccountId, AcctAlias, Model, Currency, ReportDate,
                     Cash, CashLong, CashShort,
                     Stock, StockLong, StockShort,
                     Funds, FundsLong, FundsShort,
                     DividendAccruals, DividendAccrualsLong, DividendAccrualsShort,
                     Total, TotalLong, TotalShort)
                    VALUES
                    (@accountId, @acctAlias, @model, @currency, @reportDate,
                     @cash, @cashLong, @cashShort,
                     @stock, @stockLong, @stockShort,
                     @funds, @fundsLong, @fundsShort,
                     @dividendAccruals, @dividendAccrualsLong, @dividendAccrualsShort,
                     @total, @totalLong, @totalShort);

                    SELECT CAST(SCOPE_IDENTITY() as int);";
    }
}
