using TraderView.Application.Specifications;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.Positions.Specifications;

/// <summary>
/// Specification for retrieving an open position by instrument ID
/// </summary>
public class OpenPositionByInstrumentIdSpecification : BaseSpecification<Position>
{
    public OpenPositionByInstrumentIdSpecification(int instrumentId)
    {
        Criteria = x => x.InstrumentId == instrumentId && x.Status == "Open";
        AddInclude(x => x.Instrument);
        AddInclude(x => x.TradeExecutions);
    }
}
