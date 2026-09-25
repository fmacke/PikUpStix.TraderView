using TraderView.Application.Specifications;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.TradeExecutions.Specifications;

/// <summary>
/// Specification for retrieving all trade executions, ordered by trade date and time
/// </summary>
public class AllTradeExecutionsSpecification : BaseSpecification<TradeExecution>
{
    public AllTradeExecutionsSpecification()
    {
        ApplyOrdering(x => x.TradeDate, isDescending: false);
        ApplyOrdering(x => x.DateTime, isDescending: false);
    }
}
