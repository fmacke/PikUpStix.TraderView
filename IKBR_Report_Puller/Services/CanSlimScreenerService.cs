using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities.FMP;

namespace PikUpStix.TraderView.Services
{
    public class CanSlimScreenerService : ICanSlimScreenerService
    {
        private readonly ICanSlimCandidateRepository _canSlimCandidateRepository;
        public CanSlimScreenerService(ICanSlimCandidateRepository canSlimCandidateRepository)
        {
            _canSlimCandidateRepository = canSlimCandidateRepository;
        }
        async Task<List<CanSlimCandidate>> ICanSlimScreenerService.GetAllBySnapshotIdAsync(int snapshotId)
        {
            return await _canSlimCandidateRepository.GetAllBySnapshotIdAsync(snapshotId);
        }
        async Task<int> ICanSlimScreenerService.CreateCanSlimScreenerSnapshot(List<CanSlimCandidate> candidates)
        {
            var snapshotId = await _canSlimCandidateRepository.InsertScreenerSnapshotAsync();
            foreach (var candidate in candidates)
            {
                candidate.CanSlimScreenerSnapshotId = snapshotId;
                await _canSlimCandidateRepository.InsertAsync(candidate);
            }
            return snapshotId;
        }
        async Task<CanSlimScreenerSnapshot> ICanSlimScreenerService.GetLatestScreenerSnapShot()
        {
            return await _canSlimCandidateRepository.GetLatestScreenerSnapshotAsync();
        }

        public Task<List<CanSlimCandidate>> GetAllBySnapshotIdAsync(int snapshotId)
        {
            throw new NotImplementedException();
        }

        public Task<int> CreateCanSlimScreenerSnapshot(List<CanSlimCandidate> candidates)
        {
            throw new NotImplementedException();
        }

        public Task<CanSlimScreenerSnapshot> GetLatestScreenerSnapShot()
        {
            throw new NotImplementedException();
        }
    }
}
