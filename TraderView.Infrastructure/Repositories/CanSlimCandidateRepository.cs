using Microsoft.Data.SqlClient;
using TraderView.Application.Features.CanSlimScreener.Command;
using TraderView.Application.Features.CanSlimScreener.Query.GetBy;
using TraderView.Application.Features.Instruments.Query.GetBy;
using TraderView.Application.Interfaces.Repositories;
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
                    query: new GetCandidatesBySnapshotIdQuery(snapshotId).Script(),
                    mapFunction: reader => new CanSlimCandidate
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Symbol = reader.GetString(reader.GetOrdinal("Symbol")),
                        CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
                        Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                        Volume = reader.GetDecimal(reader.GetOrdinal("Volume")),
                        MarketCap = reader.GetDecimal(reader.GetOrdinal("MarketCap")),
                        Exchange = reader.GetString(reader.GetOrdinal("Exchange")),
                        Sector = reader.GetString(reader.GetOrdinal("Sector")),
                        Industry = reader.GetString(reader.GetOrdinal("Industry")),
                        CurrentQuarterLatestQuarterDate = reader.GetString(reader.GetOrdinal("CurrentQuarter_LatestQuarterDate")),
                        CurrentQuarterLatestQuarterEps = reader.GetDecimal(reader.GetOrdinal("CurrentQuarter_LatestQuarterEps")),
                        CurrentQuarterPriorYearQuarterEps = reader.GetDecimal(reader.GetOrdinal("CurrentQuarter_PriorYearQuarterEps")),
                        CurrentQuarterEpsGrowthYoYpercent = reader.GetDecimal(reader.GetOrdinal("CurrentQuarter_EpsGrowthYoYPercent")),
                        CurrentQuarterRevenueGrowthYoYpercent = reader.GetDecimal(reader.GetOrdinal("CurrentQuarter_RevenueGrowthYoYPercent")),
                        CurrentQuarterIsAccelerating = reader.GetBoolean(reader.GetOrdinal("CurrentQuarter_IsAccelerating")),
                        CurrentQuarterPassesCriteria = reader.GetBoolean(reader.GetOrdinal("CurrentQuarter_PassesCriteria")),
                        EvaluationDateUtc = reader.GetDateTime(reader.GetOrdinal("EvaluationDateUtc")),
                        AnnualEpsCagr3YearPercent = reader.GetDecimal(reader.GetOrdinal("Annual_EpsCagr3YearPercent")),
                        AnnualEpsCagr5YearPercent = reader.IsDBNull(reader.GetOrdinal("Annual_EpsCagr5YearPercent")) ? null : reader.GetDecimal(reader.GetOrdinal("Annual_EpsCagr5YearPercent")),
                        AnnualReturnOnEquityPercent = reader.GetDecimal(reader.GetOrdinal("Annual_ReturnOnEquityPercent")),
                        AnnualHasConsecutiveAnnualGrowth = reader.GetBoolean(reader.GetOrdinal("Annual_HasConsecutiveAnnualGrowth")),
                        AnnualLatestFiscalYearEps = reader.GetDecimal(reader.GetOrdinal("Annual_LatestFiscalYearEps")),
                        AnnualLatestFiscalYear = reader.GetString(reader.GetOrdinal("Annual_LatestFiscalYear")),
                        AnnualPriorYear1Eps = reader.GetDecimal(reader.GetOrdinal("Annual_PriorYear1Eps")),
                        AnnualPriorYear2Eps = reader.GetDecimal(reader.GetOrdinal("Annual_PriorYear2Eps")),
                        AnnualPriorYear3Eps = reader.GetDecimal(reader.GetOrdinal("Annual_PriorYear3Eps")),
                        AnnualOperatingMarginPercent = reader.GetDecimal(reader.GetOrdinal("Annual_OperatingMarginPercent")),
                        AnnualReturnOnAssetsPercent = reader.GetDecimal(reader.GetOrdinal("Annual_ReturnOnAssetsPercent")),
                        AnnualPassesCriteria = reader.GetBoolean(reader.GetOrdinal("Annual_PassesCriteria")),
                        AnnualFundamentalGrade = reader.GetString(reader.GetOrdinal("Annual_FundamentalGrade"))
                        // Annual History is not included in this query; it would require a separate query to fetch the annual history for each candidate.
                    });
                return candidates;
            });
        }

        CanSlimScreenerSnapshot? ICanSlimCandidateRepository.GetLatestScreenerSnapShot()
        {
            return ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    var result = ExecuteSingle<CanSlimScreenerSnapshot>(connection, transaction, new GetLatestScreenerSnapshot().Script(), MapScreenerSnapshot);                  
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
                    var parameters = TradeParameterBuilder.GetCanSlimCandidate(candidate);
                    var insertQuery = new CreateCanSlimCandidateCommand().Script();
                    var canSlimCandidateId = ExecuteScalar<int>(connection, transaction, insertQuery, parameters);

                    foreach (var annualHistory in candidate.CanSlimCandidateAnnualHistories)
                    {
                        annualHistory.CandidateId = canSlimCandidateId;
                        parameters = TradeParameterBuilder.GetCanSlimAnnualHistory(annualHistory);
                        insertQuery = new CreateCanSlimCandidateAnnualHistoryCommand().Script();
                        ExecuteScalar<int>(connection, transaction, insertQuery, parameters);
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
                    var parameters = new Dictionary<string, object>
                        {{ "@CreatedAt", DateTime.Now } };
                    var insertQuery = new CreateCanSlimScreentSnapshotCommand().Script();
                    int canSlimScreenerSnapShotId = ExecuteScalar<int>(connection, transaction, insertQuery, parameters);
                    transaction.Commit();
                    return canSlimScreenerSnapShotId;
                }
            });
        }
        private static CanSlimScreenerSnapshot MapScreenerSnapshot(SqlDataReader reader)
        {
            return new CanSlimScreenerSnapshot
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
 }
