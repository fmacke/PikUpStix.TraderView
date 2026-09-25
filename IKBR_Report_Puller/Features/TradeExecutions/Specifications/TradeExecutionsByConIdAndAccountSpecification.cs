using System.Linq.Expressions;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.TradeExecutions.Specifications;

/// <summary>
/// Specification for retrieving trade executions by ConId and AccountId
/// Ordered by trade date and time
/// </summary>
public class TradeExecutionsByConIdAndAccountSpecification : ISpecification<TradeExecution>
{
    private readonly string _conid;
    private readonly string _accountId;

    public TradeExecutionsByConIdAndAccountSpecification(long? conid, string accountId)
    {
        _conid = conid?.ToString() ?? "";
        _accountId = accountId;
    }

    public string ConId => _conid;
    public string AccountId => _accountId;

    public Expression<Func<TradeExecution, bool>>? Criteria => x => 
        x.Conid == _conid && x.AccountId == _accountId;

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
