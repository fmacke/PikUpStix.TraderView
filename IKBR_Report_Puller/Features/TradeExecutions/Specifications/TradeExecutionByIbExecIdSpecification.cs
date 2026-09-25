using TraderView.Application.Specifications;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.TradeExecutions.Specifications;

/// <summary>
/// Specification for retrieving a trade execution by its IB execution ID
/// </summary>
public class TradeExecutionByIbExecIdSpecification : BaseSpecification<TradeExecution>
{
    public TradeExecutionByIbExecIdSpecification(string ibExecId)
    {
        Criteria = x => x.IbExecId == ibExecId;
    }
}
