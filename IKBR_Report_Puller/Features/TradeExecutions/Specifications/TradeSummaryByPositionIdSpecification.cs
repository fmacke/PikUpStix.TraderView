using System.Linq.Expressions;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.TradeExecutions.Specifications;

/// <summary>
/// Specification for calculating trade summary for a position
/// This is a complex aggregation that requires raw SQL or advanced LINQ
/// </summary>
public class TradeSummaryByPositionIdSpecification : ISpecification<TradeExecution>
{
    private readonly int _positionId;

    public TradeSummaryByPositionIdSpecification(int positionId)
    {
        _positionId = positionId;
    }

    public int PositionId => _positionId;

    public Expression<Func<TradeExecution, bool>>? Criteria => x => x.PositionId == _positionId;

    public List<Expression<Func<TradeExecution, object>>> Includes => new()
    {
        x => x.Position
    };

    public List<string> IncludeStrings => new();

    public List<(Expression<Func<TradeExecution, object>> KeySelector, bool IsDescending)> OrderBys => new()
    {
        (x => x.TradeDate, false),
        (x => x.DateTime, false)
    };

    public int? Take => null;

    public int? Skip => null;

    public bool IsPagingEnabled => false;
}
