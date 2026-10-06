using TraderView.Application.Specifications;
using TraderView.Domain.Entities;

namespace TraderView.Application.Specifications.PositionCalculators
{
    public class PositionCalculatorCandidatesSpecification : BaseSpecification<PositionCalculator>
    {
        public PositionCalculatorCandidatesSpecification(int positionId)
        {
            // Candidates are calculators that are either unlinked (PositionId == null)
            // or already linked to the target position (so they appear in the list)
            Criteria = pc => pc.PositionId == null || pc.PositionId == positionId;
        }
    }
}
