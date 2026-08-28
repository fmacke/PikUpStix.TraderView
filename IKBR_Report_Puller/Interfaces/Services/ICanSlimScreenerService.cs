using TraderView.Domain.Entities.FMP;

namespace TraderView.Application.Interfaces.Services
{
    public interface ICanSlimScreenerService
    {
        Task<List<CanSlimCandidate>> GetAllBySnapshotIdAsync(int snapshotId);
        Task<int> CreateCanSlimScreenerSnapshot(List<CanSlimCandidate> candidates);
        Task<CanSlimScreenerSnapshot> GetLatestScreenerSnapShot();
    }
}