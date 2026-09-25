using TraderView.Application.Interfaces.Persistence;
using TraderView.Domain.Entities.FMP;

namespace TraderView.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for CanSlimCandidate entities
    /// </summary>
    public interface ICanSlimCandidateRepository : IRepository<CanSlimCandidate>
    {
        /// <summary>
        /// Retrieves all CanSlimCandidates for a given screener snapshot
        /// </summary>
        Task<List<CanSlimCandidate>> GetAllBySnapshotIdAsync(int snapshotId);

        /// <summary>
        /// Retrieves the latest CanSlimScreenerSnapshot
        /// </summary>
        Task<CanSlimScreenerSnapshot?> GetLatestScreenerSnapshotAsync();

        /// <summary>
        /// Inserts a new CanSlimCandidate with its associated annual histories
        /// </summary>
        Task<int> InsertAsync(CanSlimCandidate candidate);

        /// <summary>
        /// Creates a new CanSlimScreenerSnapshot
        /// </summary>
        Task<int> InsertScreenerSnapshotAsync();
    }
}