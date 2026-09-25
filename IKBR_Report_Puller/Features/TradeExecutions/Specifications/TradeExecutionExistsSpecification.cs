using TraderView.Application.Specifications;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.TradeExecutions.Specifications;

/// <summary>
/// Specification for checking if a trade execution exists by IB execution ID
/// </summary>
public class TradeExecutionExistsSpecification : BaseSpecification<TradeExecution>
{
    public TradeExecutionExistsSpecification(string ibExecId)
    {
        Criteria = x => x.IbExecId == ibExecId;
    }
}
