using TraderView.Application.Specifications;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.TradeExecutions.Specifications;

/// <summary>
/// Specification for calculating trade summary for a position
/// This is a complex aggregation that requires raw SQL or advanced LINQ
/// </summary>
public class TradeSummaryByPositionIdSpecification : BaseSpecification<TradeExecution>
{
    public int PositionId { get; }

    public TradeSummaryByPositionIdSpecification(int positionId)
    {
        PositionId = positionId;
        Criteria = x => x.PositionId == positionId;
        AddInclude(x => x.Position);
        ApplyOrdering(x => x.TradeDate, isDescending: false);
        ApplyOrdering(x => x.DateTime, isDescending: false);
    }
}
