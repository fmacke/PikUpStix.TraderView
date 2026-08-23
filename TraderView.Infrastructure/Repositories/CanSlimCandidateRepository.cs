using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection.Metadata.Ecma335;
using TraderView.Application.Features.CanSlimScreener.Command;
using TraderView.Application.Features.Instruments.Query.GetBy;
using TraderView.Application.Features.TradeExecutions.Command.Create;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Models.FMP;
using TraderView.Domain.Entities;
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
                return candidates;
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

                    foreach (var annualHistory in candidate.Annual.AnnualHistory)
                    {
                        annualHistory.CandidateId = canSlimCandidateId;
                        parameters = TradeParameterBuilder.GetCanSlimAnnualHistory(annualHistory);
                        insertQuery = new CreateCanSlimCandidateAnnualHistoryCommand().Script();
                        ExecuteScalar<int>(connection, transaction, insertQuery, parameters);
                    }
                    transaction.Commit();
                    Console.WriteLine($"Created new CanSlimCandidate (Id: {canSlimCandidateId}) for symbol {candidate.Symbol}, on {candidate.Annual.EvaluationDateUtc:yyyy-MM-dd}");
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
    }
 }
