using Microsoft.Data.SqlClient;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Domain.Entities;

namespace TraderView.Infrastructure.Repositories
{
    public class EquitySummaryRepository : BaseRepository, IEquitySummaryRepository
    {
        public EquitySummaryRepository(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Gets all equity summaries
        /// </summary>
        public List<EquitySummary> GetAll()
        {
            return ExecuteDatabaseOperation(connection =>
            {
                return ExecuteList(connection, null, MapReaderToEntity, new TraderView.Application.Features.EquitySummaries.Query.Get.GetAllEquitySummariesSqlQuery());
            });
        }

        /// <summary>
        /// Gets an equity summary by its ID
        /// </summary>
        public EquitySummary? GetById(int id)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                return ExecuteSingle(connection, null, MapReaderToEntity, new TraderView.Application.Features.EquitySummaries.Query.GetBy.GetEquitySummaryByIdSqlQuery(id));
            });
        }

        /// <summary>
        /// Gets an equity summary by account ID and report date
        /// </summary>
        public EquitySummary? GetByAccountAndDate(string accountId, DateTime reportDate)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                return ExecuteSingle(connection, null, MapReaderToEntity, new TraderView.Application.Features.EquitySummaries.Query.GetBy.GetEquitySummaryByAccountAndDateSqlQuery(accountId, reportDate));
            });
        }

        /// <summary>
        /// Gets all equity summaries for a specific account
        /// </summary>
        public List<EquitySummary> GetByAccountId(string accountId)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                return ExecuteList(connection, null, MapReaderToEntity, new TraderView.Application.Features.EquitySummaries.Query.GetBy.GetEquitySummariesByAccountIdSqlQuery(accountId));
            });
        }

        /// <summary>
        /// Gets all equity summaries for a date range
        /// </summary>
        public List<EquitySummary> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                return ExecuteList(connection, null, MapReaderToEntity, new TraderView.Application.Features.EquitySummaries.Query.GetBy.GetEquitySummariesByDateRangeSqlQuery(startDate, endDate));
            });
        }

        /// <summary>
        /// Creates a new equity summary and returns the new ID
        /// </summary>
        public int Create(EquitySummary equitySummary)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    var id = ExecuteScalar<int>(connection, transaction, new TraderView.Application.Features.EquitySummaries.Command.Create.InsertEquitySummarySqlCommand(equitySummary));
                    transaction.Commit();
                    return id;
                }
            });
        }

        /// <summary>
        /// Updates an existing equity summary
        /// </summary>
        public void Update(EquitySummary equitySummary)
        {
            ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    ExecuteCommand(connection, transaction, new TraderView.Application.Features.EquitySummaries.Command.Update.UpdateEquitySummarySqlCommand(equitySummary));
                    transaction.Commit();
                }
            });
        }

        /// <summary>
        /// Deletes an equity summary by its ID
        /// </summary>
        public bool Delete(int id)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    var rows = ExecuteScalar<int>(connection, transaction, new TraderView.Application.Features.EquitySummaries.Command.Delete.DeleteEquitySummarySqlCommand(id));
                    transaction.Commit();
                    return rows > 0;
                }
            });
        }

        /// <summary>
        /// Maps a data reader row to an EquitySummary entity
        /// </summary>
        private static EquitySummary MapReaderToEntity(SqlDataReader reader)
        {
            return new EquitySummary
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                AccountId = reader.GetString(reader.GetOrdinal("AccountId")),
                AcctAlias = reader.IsDBNull(reader.GetOrdinal("AcctAlias")) ? null : reader.GetString(reader.GetOrdinal("AcctAlias")),
                Model = reader.IsDBNull(reader.GetOrdinal("Model")) ? null : reader.GetString(reader.GetOrdinal("Model")),
                Currency = reader.GetString(reader.GetOrdinal("Currency")),
                ReportDate = reader.GetDateTime(reader.GetOrdinal("ReportDate")),
                Cash = reader.GetDecimal(reader.GetOrdinal("Cash")),
                CashLong = reader.GetDecimal(reader.GetOrdinal("CashLong")),
                CashShort = reader.GetDecimal(reader.GetOrdinal("CashShort")),
                Stock = reader.GetDecimal(reader.GetOrdinal("Stock")),
                StockLong = reader.GetDecimal(reader.GetOrdinal("StockLong")),
                StockShort = reader.GetDecimal(reader.GetOrdinal("StockShort")),
                Funds = reader.GetDecimal(reader.GetOrdinal("Funds")),
                FundsLong = reader.GetDecimal(reader.GetOrdinal("FundsLong")),
                FundsShort = reader.GetDecimal(reader.GetOrdinal("FundsShort")),
                DividendAccruals = reader.GetDecimal(reader.GetOrdinal("DividendAccruals")),
                DividendAccrualsLong = reader.GetDecimal(reader.GetOrdinal("DividendAccrualsLong")),
                DividendAccrualsShort = reader.GetDecimal(reader.GetOrdinal("DividendAccrualsShort")),
                Total = reader.GetDecimal(reader.GetOrdinal("Total")),
                TotalLong = reader.GetDecimal(reader.GetOrdinal("TotalLong")),
                TotalShort = reader.GetDecimal(reader.GetOrdinal("TotalShort")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }

        /// <summary>
        /// Maps all rows from a data reader to a list of EquitySummary entities
        /// </summary>
        private static List<EquitySummary> MapReaderToList(SqlDataReader reader)
        {
            var summaries = new List<EquitySummary>();
            while (reader.Read())
            {
                summaries.Add(MapReaderToEntity(reader));
            }
            return summaries;
        }
    }
}
