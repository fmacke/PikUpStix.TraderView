using TraderView.Application.Specifications;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.Positions.Specifications;

/// <summary>
/// Specification for retrieving all open positions with their related instrument data
/// </summary>
public class OpenPositionsSpecification : BaseSpecification<Position>
{
    public OpenPositionsSpecification()
    {
        Criteria = x => x.Status == "Open" && x.Instrument != null && x.Instrument.ContractUnitType != "CASH";
        AddInclude(x => x.Instrument);
        AddInclude(x => x.TradeExecutions);
        ApplyOrdering(x => x.OpenDate, isDescending: true);
    }
}
