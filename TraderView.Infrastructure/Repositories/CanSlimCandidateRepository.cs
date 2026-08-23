using Microsoft.Data.SqlClient;
using TraderView.Application.Features.Instruments.Query.GetBy;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Models.FMP;
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
                var candidates = new List<CanSlimCandidate>();
                var query = new GetCandidatesBySnapshotIdQuery().Script();
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CanSlimScreenerSnapshotId", snapshotId);
                stopped here - checked this against pattern elsewhere
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    candidates.Add(new CanSlimCandidate
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
                        CurrentQuarter = new CanSlimCurrentQuarterMetric
                        {
                            Symbol = reader.GetString(reader.GetOrdinal("Symbol")),
                            LatestQuarterDate = reader.GetString(reader.GetOrdinal("LatestQuarterDate")),
                            LatestQuarterEps = reader.GetDecimal(reader.GetOrdinal("LatestQuarterEps")),
                            PriorYearQuarterEps = reader.GetDecimal(reader.GetOrdinal("PriorYearQuarterEps")),
                            EpsGrowthYoYPercent = reader.GetDecimal(reader.GetOrdinal("EpsGrowthYoYPercent")),
                            RevenueGrowthYoYPercent = reader.GetDecimal(reader.GetOrdinal("RevenueGrowthYoYPercent")),
                            IsAccelerating = reader.GetBoolean(reader.GetOrdinal("IsAccelerating")),
                            PassesCriteria = reader.GetBoolean(reader.GetOrdinal("PassesCriteria"))
                        },
                        Annual = new CanSlimAnnualMetric
                        {
                            Symbol = reader.GetString(reader.GetOrdinal("Symbol")),
                            EvaluationDateUtc = reader.GetDateTime(reader.GetOrdinal("EvaluationDateUtc")),
                            EpsCagr3YearPercent = reader.GetDecimal(reader.GetOrdinal("EpsCagr3YearPercent")),
                            EpsCagr5YearPercent = reader.IsDBNull(reader.GetOrdinal("EpsCagr5YearPercent")) ? null : reader.GetDecimal(reader.GetOrdinal("EpsCagr5YearPercent")),
                            ReturnOnEquityPercent = reader.GetDecimal(reader.GetOrdinal("ReturnOnEquityPercent")),
                            HasConsecutiveAnnualGrowth = reader.GetBoolean(reader.GetOrdinal("HasConsecutiveAnnualGrowth")),
                            LatestFiscalYearEps = reader.GetDecimal(reader.GetOrdinal("LatestFiscalYearEps")),
                            LatestFiscalYear = reader.GetString(reader.GetOrdinal("LatestFiscalYear")),
                            PriorYear1Eps = reader.GetDecimal(reader.GetOrdinal("PriorYear1Eps")),
                            PriorYear2Eps = reader.GetDecimal(reader.GetOrdinal("PriorYear2Eps")),
                            PriorYear3Eps = reader.GetDecimal(reader.GetOrdinal("PriorYear3Eps")),
                            OperatingMarginPercent = reader.GetDecimal(reader.GetOrdinal("OperatingMarginPercent")),
                            ReturnOnAssetsPercent = reader.GetDecimal(reader.GetOrdinal("ReturnOnAssetsPercent")),
                            PassesCriteria = reader.GetBoolean(reader.GetOrdinal("PassesCriteria")),
                            FundamentalGrade = reader.GetString(reader.GetOrdinal("FundamentalGrade"))
                            // Annual History is not included in this query; it would require a separate query to fetch the annual history for each candidate.
                        }
                    });
                }
                return candidates;
            });
        }

        int ICanSlimCandidateRepository.Insert(CanSlimCandidate candidate)
        {
            throw new NotImplementedException();
        }
    }
}
