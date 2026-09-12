namespace TraderView.Application.Features.EquitySummaries.Command.Update;

/// <summary>
/// Command to update an existing equity summary
/// </summary>
public class UpdateEquitySummaryCommand
{
    public int Id { get; set; }

    public string AccountId { get; set; } = null!;

    public string? AcctAlias { get; set; }

    public string? Model { get; set; }

    public string Currency { get; set; } = null!;

    public DateTime ReportDate { get; set; }

    // Cash Components
    public decimal Cash { get; set; }

    public decimal CashLong { get; set; }

    public decimal CashShort { get; set; }

    // Major Asset Classes
    public decimal Stock { get; set; }

    public decimal StockLong { get; set; }

    public decimal StockShort { get; set; }

    public decimal Funds { get; set; }

    public decimal FundsLong { get; set; }

    public decimal FundsShort { get; set; }

    // Accruals & Adjustments
    public decimal DividendAccruals { get; set; }

    public decimal DividendAccrualsLong { get; set; }

    public decimal DividendAccrualsShort { get; set; }

    // Totals
    public decimal Total { get; set; }

    public decimal TotalLong { get; set; }

    public decimal TotalShort { get; set; }

    /// <summary>
    /// Converts the command to an EquitySummary entity
    /// </summary>
    public TraderView.Domain.Entities.EquitySummary ToEntity()
    {
        return new TraderView.Domain.Entities.EquitySummary
        {
            Id = Id,
            AccountId = AccountId,
            AcctAlias = AcctAlias,
            Model = Model,
            Currency = Currency,
            ReportDate = ReportDate,
            Cash = Cash,
            CashLong = CashLong,
            CashShort = CashShort,
            Stock = Stock,
            StockLong = StockLong,
            StockShort = StockShort,
            Funds = Funds,
            FundsLong = FundsLong,
            FundsShort = FundsShort,
            DividendAccruals = DividendAccruals,
            DividendAccrualsLong = DividendAccrualsLong,
            DividendAccrualsShort = DividendAccrualsShort,
            Total = Total,
            TotalLong = TotalLong,
            TotalShort = TotalShort
        };
    }
}
