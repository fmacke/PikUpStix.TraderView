using System.Linq.Expressions;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.TradeExecutions.Specifications;

/// <summary>
/// Specification for retrieving trade executions by position ID
/// </summary>
public class TradeExecutionByPositionIdSpecification : ISpecification<TradeExecution>
{
    private readonly int _positionId;

    public TradeExecutionByPositionIdSpecification(int positionId)
    {
        _positionId = positionId;
    }

    public Expression<Func<TradeExecution, bool>>? Criteria => x => x.PositionId == _positionId;

    public List<Expression<Func<TradeExecution, object>>> Includes => new();

    public List<string> IncludeStrings => new();

    public List<(Expression<Func<TradeExecution, object>> KeySelector, bool IsDescending)> OrderBys => new();

    public int? Take => null;

    public int? Skip => null;

    public bool IsPagingEnabled => false;
}
