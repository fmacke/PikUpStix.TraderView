using TraderView.Domain.Entities;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Models.FMP;

namespace PikUpStix.TraderView.Services
{
    public class TradeExecutionService : ITradeExecutionService
    {
        private readonly ITradeExecutionRepository _tradeExecutionRepository;

        public TradeExecutionService(ITradeExecutionRepository tradeExecutionRepository, IPositionRepository positionRepository)
        {
            _tradeExecutionRepository = tradeExecutionRepository;
        }

        async Task<List<Position>> ITradeExecutionService.GetOpenPositionsAsync()
        {
            return await Task.Run(() => _tradeExecutionRepository.GetOpenPositions());
        }
    }

    public class CanSlimScreenerService : ICanSlimScreenerService
    {
        private readonly ICanSlimCandidateRepository _canSlimCandidateRepository;
        public CanSlimScreenerService(ICanSlimCandidateRepository canSlimCandidateRepository)
        {
            _canSlimCandidateRepository = canSlimCandidateRepository;
        }
        async Task<List<CanSlimCandidate>> ICanSlimScreenerService.GetAllBySnapshotIdAsync(int snapshotId)
        {
            return await Task.Run(() => _canSlimCandidateRepository.GetAllBySnapshotId(snapshotId));
        }
        async Task<int> ICanSlimScreenerService.CreateCanSlimScreenerSnapshot(List<CanSlimCandidate> candidates)
        {
            var snapshotId = await Task.Run(() => _canSlimCandidateRepository.InsertScreenerSnapShot());
            foreach (var candidate in candidates)
            {
                candidate.CanSlimScreenerSnapShotId = snapshotId;
                await Task.Run(() => _canSlimCandidateRepository.Insert(candidate));
            }
            return snapshotId;
        }
    }
}
