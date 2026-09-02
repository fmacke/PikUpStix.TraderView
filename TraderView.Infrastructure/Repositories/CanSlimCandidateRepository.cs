using Microsoft.Data.SqlClient;
using TraderView.Application.Features.CanSlimScreener.Command;
using TraderView.Application.Features.CanSlimScreener.Query.GetBy;
using TraderView.Application.Features.Instruments.Query.GetBy;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Mappers;
using TraderView.Domain.Entities.FMP;
namespace TraderView.Infrastructure.Repositories
{
    public class CanSlimCandidateRepository : BaseRepository, ICanSlimCandidateRepository
    {
        public CanSlimCandidateRepository(string connectionString) : base(connectionString)
        {
        }

        List<CanSlimCandidate> ICanSlimCandidateRepository.GetAllBySnapshotId(int snapshotId)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var candidates = ExecuteList(
                    connection,
                    transaction: null,
                    MapFromReader.MapCanSlimCandiate,
                    new GetCandidatesBySnapshotIdQuery(snapshotId)
                    
                );
                return candidates;
            });
        }

        CanSlimScreenerSnapshot? ICanSlimCandidateRepository.GetLatestScreenerSnapShot()
        {
            return ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    var result = ExecuteSingle<CanSlimScreenerSnapshot>(connection, transaction, MapFromReader.MapScreenerSnapshot, new GetLatestScreenerSnapshot());                  
                    return result;
                }
            });
        }

        int ICanSlimCandidateRepository.Insert(CanSlimCandidate candidate)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    var canSlimCandidateId = ExecuteScalar<int>(connection, transaction, new CreateCanSlimCandidateCommand(candidate));

                    foreach (var annualHistory in candidate.CanSlimCandidateAnnualHistories)
                    {
                        annualHistory.CandidateId = canSlimCandidateId;
                        ExecuteScalar<int>(connection, transaction, new CreateCanSlimCandidateAnnualHistoryCommand(annualHistory));
                    }
                    transaction.Commit();
                    Console.WriteLine($"Created new CanSlimCandidate (Id: {canSlimCandidateId}) for symbol {candidate.Symbol}, on {candidate.EvaluationDateUtc:yyyy-MM-dd}");
                    return canSlimCandidateId;
                }
            });
        }

        int ICanSlimCandidateRepository.InsertScreenerSnapShot()
        {
            return ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    int canSlimScreenerSnapShotId = ExecuteScalar<int>(connection, transaction, new CreateCanSlimScreenSnapshotCommand());
                    transaction.Commit();
                    return canSlimScreenerSnapShotId;
                }
            });
        }
    }
 }
