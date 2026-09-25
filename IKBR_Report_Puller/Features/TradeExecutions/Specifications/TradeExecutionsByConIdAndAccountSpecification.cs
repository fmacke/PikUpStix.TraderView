using TraderView.Application.Specifications;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.TradeExecutions.Specifications;

/// <summary>
/// Specification for retrieving trade executions by ConId and AccountId
/// Ordered by trade date and time
/// </summary>
public class TradeExecutionsByConIdAndAccountSpecification : BaseSpecification<TradeExecution>
{
    public string ConId { get; }
    public string AccountId { get; }

    public TradeExecutionsByConIdAndAccountSpecification(long? conid, string accountId)
    {
        ConId = conid?.ToString() ?? "";
        AccountId = accountId;
        Criteria = x => x.Conid == ConId && x.AccountId == AccountId;
        ApplyOrdering(x => x.TradeDate, isDescending: false);
        ApplyOrdering(x => x.DateTime, isDescending: false);
    }
}
