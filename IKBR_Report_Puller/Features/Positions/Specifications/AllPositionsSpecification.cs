using TraderView.Application.Specifications;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.Positions.Specifications;

/// <summary>
/// Specification for retrieving all positions with their related instrument data
/// </summary>
public class AllPositionsSpecification : BaseSpecification<Position>
{
    public AllPositionsSpecification()
    {
        AddInclude(x => x.Instrument);
        AddInclude(x => x.TradeExecutions);
        ApplyOrdering(x => x.OpenDate, isDescending: true);
    }
}
