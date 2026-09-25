using System.Linq.Expressions;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.TradeExecutions.Specifications;

/// <summary>
/// Specification for retrieving all trade executions, ordered by trade date and time
/// </summary>
public class AllTradeExecutionsSpecification : ISpecification<TradeExecution>
{
    public Expression<Func<TradeExecution, bool>>? Criteria => null;

    public List<Expression<Func<TradeExecution, object>>> Includes => new();

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
