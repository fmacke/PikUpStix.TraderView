using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Specifications;
using TraderView.Domain.Entities.FMP;
using TraderView.Infrastructure.DbContexts;

namespace TraderView.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for managing CanSlimCandidate entities using Entity Framework Core
    /// </summary>
    public class CanSlimCandidateRepository : EfBaseRepository<CanSlimCandidate>, ICanSlimCandidateRepository
    {
        private readonly EfBaseRepository<CanSlimScreenerSnapshot> _snapshotRepository;
        private readonly EfBaseRepository<CanSlimCandidateAnnualHistory> _annualHistoryRepository;

        public CanSlimCandidateRepository(AppDbContext db) : base(db)
        {
            _snapshotRepository = new EfBaseRepository<CanSlimScreenerSnapshot>(db);
            _annualHistoryRepository = new EfBaseRepository<CanSlimCandidateAnnualHistory>(db);
        }

        /// <summary>
        /// Retrieves all CanSlimCandidates for a given screener snapshot
        /// </summary>
        public async Task<List<CanSlimCandidate>> GetAllBySnapshotIdAsync(int snapshotId)
        {
            var specification = new GetCanSlimCandidatesBySnapshotIdSpecification(snapshotId);
            var candidates = await GetAsync(specification);
            return candidates.ToList();
        }

        /// <summary>
        /// Retrieves the latest CanSlimScreenerSnapshot
        /// </summary>
        public async Task<CanSlimScreenerSnapshot?> GetLatestScreenerSnapshotAsync()
        {
            var specification = new GetLatestScreenerSnapshotSpecification();
            return await _snapshotRepository.GetSingleAsync(specification);
        }

        /// <summary>
        /// Inserts a new CanSlimCandidate and its associated annual histories
        /// </summary>
        public async Task<int> InsertAsync(CanSlimCandidate candidate)
        {
            // Add the candidate to the database
            var addedCandidate = await AddAsync(candidate);

            // Add annual histories if any exist
            if (addedCandidate.CanSlimCandidateAnnualHistories != null && addedCandidate.CanSlimCandidateAnnualHistories.Count > 0)
            {
                foreach (var annualHistory in addedCandidate.CanSlimCandidateAnnualHistories)
                {
                    annualHistory.CandidateId = addedCandidate.Id;
                    await _annualHistoryRepository.AddAsync(annualHistory);
                }
            }

            Console.WriteLine($"Created new CanSlimCandidate (Id: {addedCandidate.Id}) for symbol {addedCandidate.Symbol}, on {addedCandidate.EvaluationDateUtc:yyyy-MM-dd}");

            return addedCandidate.Id;
        }

        /// <summary>
        /// Creates a new CanSlimScreenerSnapshot
        /// </summary>
        public async Task<int> InsertScreenerSnapshotAsync()
        {
            var snapshot = new CanSlimScreenerSnapshot
            {
                CreatedAt = DateTime.UtcNow
            };

            var addedSnapshot = await _snapshotRepository.AddAsync(snapshot);
            return addedSnapshot.Id;
        }
    }
}
