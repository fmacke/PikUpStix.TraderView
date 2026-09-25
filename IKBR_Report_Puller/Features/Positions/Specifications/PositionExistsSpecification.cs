using TraderView.Application.Specifications;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.Positions.Specifications;

/// <summary>
/// Specification for checking if a position exists
/// </summary>
public class PositionExistsSpecification : BaseSpecification<Position>
{
    public PositionExistsSpecification(int positionId)
    {
        Criteria = x => x.Id == positionId;
    }
}
