using TraderView.Domain.Entities.FMP;

namespace TraderView.Application.Specifications.CanSlimCandidates
{
    /// <summary>
    /// Specification for retrieving the latest CanSlimScreenerSnapshot
    /// </summary>
    public class GetLatestScreenerSnapshotSpecification : BaseSpecification<CanSlimScreenerSnapshot>
    {
        public GetLatestScreenerSnapshotSpecification()
        {
            ApplyOrdering(s => s.CreatedAt, isDescending: true);
            ApplyPaging(skip: 0, take: 1);
        }
    }
}
