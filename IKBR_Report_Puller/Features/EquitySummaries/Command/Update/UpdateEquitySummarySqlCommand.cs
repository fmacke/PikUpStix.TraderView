using TraderView.Application.Features;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.EquitySummaries.Command.Update
{
    public class UpdateEquitySummarySqlCommand : IQueryWithParameters
    {
        private readonly EquitySummary _entity;
        public UpdateEquitySummarySqlCommand(EquitySummary entity)
        {
            _entity = entity;
        }

        public Dictionary<string, object> Parameters => new Dictionary<string, object>
        {
            { "@id", _entity.Id },
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

        public string Script => @"UPDATE [dbo].[EquitySummaries]
                    SET 
                        AcctAlias = @acctAlias,
                        Model = @model,
                        Currency = @currency,
                        ReportDate = @reportDate,
                        Cash = @cash,
                        CashLong = @cashLong,
                        CashShort = @cashShort,
                        Stock = @stock,
                        StockLong = @stockLong,
                        StockShort = @stockShort,
                        Funds = @funds,
                        FundsLong = @fundsLong,
                        FundsShort = @fundsShort,
                        DividendAccruals = @dividendAccruals,
                        DividendAccrualsLong = @dividendAccrualsLong,
                        DividendAccrualsShort = @dividendAccrualsShort,
                        Total = @total,
                        TotalLong = @totalLong,
                        TotalShort = @totalShort
                    WHERE Id = @id";
    }
}
