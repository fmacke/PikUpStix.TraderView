using TraderView.Domain.Entities.FMP;

namespace TraderView.Application.Specifications.CanSlimCandidates
{
    /// <summary>
    /// Specification for retrieving CanSlimCandidates by ScreenerSnapshot ID
    /// </summary>
    public class GetCanSlimCandidatesBySnapshotIdSpecification : BaseSpecification<CanSlimCandidate>
    {
        public GetCanSlimCandidatesBySnapshotIdSpecification(int snapshotId)
        {
            Criteria = candidate => candidate.CanSlimScreenerSnapshotId == snapshotId;
            AddInclude(c => c.CanSlimCandidateAnnualHistories);
        }
    }
}
