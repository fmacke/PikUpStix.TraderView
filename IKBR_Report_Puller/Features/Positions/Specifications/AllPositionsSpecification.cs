using System.Linq.Expressions;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.Positions.Specifications;

/// <summary>
/// Specification for retrieving all positions with their related instrument data
/// </summary>
public class AllPositionsSpecification : ISpecification<Position>
{
    public Expression<Func<Position, bool>>? Criteria => null;

    public List<Expression<Func<Position, object>>> Includes => new()
    {
        x => x.Instrument,
        x => x.TradeExecutions
    };

    public List<string> IncludeStrings => new();

    public List<(Expression<Func<Position, object>> KeySelector, bool IsDescending)> OrderBys => new()
    {
        (x => x.OpenDate, true)
    };

    public int? Take => null;

    public int? Skip => null;

    public bool IsPagingEnabled => false;
}
