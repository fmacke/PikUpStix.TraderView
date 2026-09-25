using TraderView.Application.Specifications;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.TradeExecutions.Specifications;

/// <summary>
/// Specification for retrieving trade executions by position ID
/// </summary>
public class TradeExecutionByPositionIdSpecification : BaseSpecification<TradeExecution>
{
    public TradeExecutionByPositionIdSpecification(int positionId)
    {
        Criteria = x => x.PositionId == positionId;
    }
}
