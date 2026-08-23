using TraderView.Application.Models.FMP;

namespace TraderView.Application.Interfaces.Repositories
{
    public interface ICanSlimCandidateRepository
    {
        List<CanSlimCandidate> GetAllBySnapshotId(int snapshotId);
        int Insert(CanSlimCandidate candidate);
        int InsertScreenerSnapShot();
    }
}