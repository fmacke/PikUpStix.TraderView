using System.Linq.Expressions;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.TradeExecutions.Specifications;

/// <summary>
/// Specification for checking if a trade execution exists by IB execution ID
/// </summary>
public class TradeExecutionExistsSpecification : ISpecification<TradeExecution>
{
    private readonly string _ibExecId;

    public TradeExecutionExistsSpecification(string ibExecId)
    {
        _ibExecId = ibExecId;
    }

    public Expression<Func<TradeExecution, bool>>? Criteria => x => x.IbExecId == _ibExecId;

    public List<Expression<Func<TradeExecution, object>>> Includes => new();

    public List<string> IncludeStrings => new();

    public List<(Expression<Func<TradeExecution, object>> KeySelector, bool IsDescending)> OrderBys => new();

    public int? Take => null;

    public int? Skip => null;

    public bool IsPagingEnabled => false;
}
