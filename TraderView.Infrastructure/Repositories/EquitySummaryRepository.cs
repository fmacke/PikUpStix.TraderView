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
                const string query = @"
                    SELECT 
                        Id, AccountId, AcctAlias, Model, Currency, ReportDate,
                        Cash, CashLong, CashShort,
                        Stock, StockLong, StockShort,
                        Funds, FundsLong, FundsShort,
                        DividendAccruals, DividendAccrualsLong, DividendAccrualsShort,
                        Total, TotalLong, TotalShort,
                        CreatedAt
                    FROM [dbo].[EquitySummaries]
                    ORDER BY ReportDate DESC, AccountId ASC";

                using (var cmd = new SqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        return MapReaderToList(reader);
                    }
                }
            });
        }

        /// <summary>
        /// Gets an equity summary by its ID
        /// </summary>
        public EquitySummary? GetById(int id)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                const string query = @"
                    SELECT 
                        Id, AccountId, AcctAlias, Model, Currency, ReportDate,
                        Cash, CashLong, CashShort,
                        Stock, StockLong, StockShort,
                        Funds, FundsLong, FundsShort,
                        DividendAccruals, DividendAccrualsLong, DividendAccrualsShort,
                        Total, TotalLong, TotalShort,
                        CreatedAt
                    FROM [dbo].[EquitySummaries]
                    WHERE Id = @id";

                var parameters = new Dictionary<string, object>
                {
                    { "@id", id }
                };

                using (var cmd = new SqlCommand(query, connection))
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapReaderToEntity(reader);
                        }
                    }
                }

                return null;
            });
        }

        /// <summary>
        /// Gets an equity summary by account ID and report date
        /// </summary>
        public EquitySummary? GetByAccountAndDate(string accountId, DateTime reportDate)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                const string query = @"
                    SELECT 
                        Id, AccountId, AcctAlias, Model, Currency, ReportDate,
                        Cash, CashLong, CashShort,
                        Stock, StockLong, StockShort,
                        Funds, FundsLong, FundsShort,
                        DividendAccruals, DividendAccrualsLong, DividendAccrualsShort,
                        Total, TotalLong, TotalShort,
                        CreatedAt
                    FROM [dbo].[EquitySummaries]
                    WHERE AccountId = @accountId
                    AND ReportDate = @reportDate";

                var parameters = new Dictionary<string, object>
                {
                    { "@accountId", accountId },
                    { "@reportDate", reportDate.Date }
                };

                using (var cmd = new SqlCommand(query, connection))
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return MapReaderToEntity(reader);
                        }
                    }
                }

                return null;
            });
        }

        /// <summary>
        /// Gets all equity summaries for a specific account
        /// </summary>
        public List<EquitySummary> GetByAccountId(string accountId)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                const string query = @"
                    SELECT 
                        Id, AccountId, AcctAlias, Model, Currency, ReportDate,
                        Cash, CashLong, CashShort,
                        Stock, StockLong, StockShort,
                        Funds, FundsLong, FundsShort,
                        DividendAccruals, DividendAccrualsLong, DividendAccrualsShort,
                        Total, TotalLong, TotalShort,
                        CreatedAt
                    FROM [dbo].[EquitySummaries]
                    WHERE AccountId = @accountId
                    ORDER BY ReportDate DESC";

                var parameters = new Dictionary<string, object>
                {
                    { "@accountId", accountId }
                };

                using (var cmd = new SqlCommand(query, connection))
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        return MapReaderToList(reader);
                    }
                }
            });
        }

        /// <summary>
        /// Gets all equity summaries for a date range
        /// </summary>
        public List<EquitySummary> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                const string query = @"
                    SELECT 
                        Id, AccountId, AcctAlias, Model, Currency, ReportDate,
                        Cash, CashLong, CashShort,
                        Stock, StockLong, StockShort,
                        Funds, FundsLong, FundsShort,
                        DividendAccruals, DividendAccrualsLong, DividendAccrualsShort,
                        Total, TotalLong, TotalShort,
                        CreatedAt
                    FROM [dbo].[EquitySummaries]
                    WHERE ReportDate >= @startDate
                    AND ReportDate <= @endDate
                    ORDER BY ReportDate DESC, AccountId ASC";

                var parameters = new Dictionary<string, object>
                {
                    { "@startDate", startDate.Date },
                    { "@endDate", endDate.Date }
                };

                using (var cmd = new SqlCommand(query, connection))
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        return MapReaderToList(reader);
                    }
                }
            });
        }

        /// <summary>
        /// Creates a new equity summary and returns its ID
        /// </summary>
        public int Create(EquitySummary equitySummary)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                const string insertQuery = @"
                    INSERT INTO [dbo].[EquitySummaries] (
                        AccountId, AcctAlias, Model, Currency, ReportDate,
                        Cash, CashLong, CashShort,
                        Stock, StockLong, StockShort,
                        Funds, FundsLong, FundsShort,
                        DividendAccruals, DividendAccrualsLong, DividendAccrualsShort,
                        Total, TotalLong, TotalShort
                    )
                    VALUES (
                        @accountId, @acctAlias, @model, @currency, @reportDate,
                        @cash, @cashLong, @cashShort,
                        @stock, @stockLong, @stockShort,
                        @funds, @fundsLong, @fundsShort,
                        @dividendAccruals, @dividendAccrualsLong, @dividendAccrualsShort,
                        @total, @totalLong, @totalShort
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                var parameters = new Dictionary<string, object>
                {
                    { "@accountId", equitySummary.AccountId },
                    { "@acctAlias", equitySummary.AcctAlias ?? (object)DBNull.Value },
                    { "@model", equitySummary.Model ?? (object)DBNull.Value },
                    { "@currency", equitySummary.Currency },
                    { "@reportDate", equitySummary.ReportDate.Date },
                    { "@cash", equitySummary.Cash },
                    { "@cashLong", equitySummary.CashLong },
                    { "@cashShort", equitySummary.CashShort },
                    { "@stock", equitySummary.Stock },
                    { "@stockLong", equitySummary.StockLong },
                    { "@stockShort", equitySummary.StockShort },
                    { "@funds", equitySummary.Funds },
                    { "@fundsLong", equitySummary.FundsLong },
                    { "@fundsShort", equitySummary.FundsShort },
                    { "@dividendAccruals", equitySummary.DividendAccruals },
                    { "@dividendAccrualsLong", equitySummary.DividendAccrualsLong },
                    { "@dividendAccrualsShort", equitySummary.DividendAccrualsShort },
                    { "@total", equitySummary.Total },
                    { "@totalLong", equitySummary.TotalLong },
                    { "@totalShort", equitySummary.TotalShort }
                };

                using (var cmd = new SqlCommand(insertQuery, connection))
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    return (int)cmd.ExecuteScalar()!;
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
                const string updateQuery = @"
                    UPDATE [dbo].[EquitySummaries]
                    SET 
                        AcctAlias = @acctAlias,
                        Model = @model,
                        Currency = @currency,
                        ReportDate = @reportDate,
                        Cash = @cash,
                        CashLong = @cashLong,
                        CashShort = @cashShort,
                        Stock = @stock,
                        StockLong = @stockLong,
                        StockShort = @stockShort,
                        Funds = @funds,
                        FundsLong = @fundsLong,
                        FundsShort = @fundsShort,
                        DividendAccruals = @dividendAccruals,
                        DividendAccrualsLong = @dividendAccrualsLong,
                        DividendAccrualsShort = @dividendAccrualsShort,
                        Total = @total,
                        TotalLong = @totalLong,
                        TotalShort = @totalShort
                    WHERE Id = @id";

                var parameters = new Dictionary<string, object>
                {
                    { "@id", equitySummary.Id },
                    { "@acctAlias", equitySummary.AcctAlias ?? (object)DBNull.Value },
                    { "@model", equitySummary.Model ?? (object)DBNull.Value },
                    { "@currency", equitySummary.Currency },
                    { "@reportDate", equitySummary.ReportDate.Date },
                    { "@cash", equitySummary.Cash },
                    { "@cashLong", equitySummary.CashLong },
                    { "@cashShort", equitySummary.CashShort },
                    { "@stock", equitySummary.Stock },
                    { "@stockLong", equitySummary.StockLong },
                    { "@stockShort", equitySummary.StockShort },
                    { "@funds", equitySummary.Funds },
                    { "@fundsLong", equitySummary.FundsLong },
                    { "@fundsShort", equitySummary.FundsShort },
                    { "@dividendAccruals", equitySummary.DividendAccruals },
                    { "@dividendAccrualsLong", equitySummary.DividendAccrualsLong },
                    { "@dividendAccrualsShort", equitySummary.DividendAccrualsShort },
                    { "@total", equitySummary.Total },
                    { "@totalLong", equitySummary.TotalLong },
                    { "@totalShort", equitySummary.TotalShort }
                };

                using (var cmd = new SqlCommand(updateQuery, connection))
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    cmd.ExecuteNonQuery();
                }

                return 0;
            });
        }

        /// <summary>
        /// Deletes an equity summary by its ID
        /// </summary>
        public bool Delete(int id)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                const string deleteQuery = @"
                    DELETE FROM [dbo].[EquitySummaries]
                    WHERE Id = @id";

                var parameters = new Dictionary<string, object>
                {
                    { "@id", id }
                };

                using (var cmd = new SqlCommand(deleteQuery, connection))
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
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
