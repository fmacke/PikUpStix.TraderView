using Microsoft.Data.SqlClient;
using PikUpStix.TraderView.Interfaces;
using PikUpStix.TraderView.Domain;
using IKBR_Report_Puller.Domain;
namespace IKBR_Report_Puller.Data.Repositories
{
    public class PositionRepository : BaseRepository, IPositionRepository
    {
        private readonly IInstrumentRepository _instrumentRepository;

        public PositionRepository(string connectionString, IInstrumentRepository instrumentRepository) : base(connectionString)
        {
            _instrumentRepository = instrumentRepository;
        }

        /// <summary>
        /// Gets all positions from the database
        /// </summary>
        List<Position> IPositionRepository.GetAllPositions()
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var positions = new List<Position>();

                using (var cmd = new SqlCommand(
                    "SELECT p.Id, p.OpenDate, p.CloseDate, p.Status, p.InstrumentId, " +
                    "i.Symbol, i.Currency, i.SecurityId " +
                    "FROM [dbo].[Positions] p " +
                    "INNER JOIN [dbo].[Instruments] i ON p.InstrumentId = i.Id " +
                    "ORDER BY p.OpenDate DESC", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            positions.Add(new Position
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                InstrumentId = reader.GetInt32(reader.GetOrdinal("InstrumentId")),
                                OpenDate = reader.GetDateTime(reader.GetOrdinal("OpenDate")),
                                CloseDate = reader.GetDateTime(reader.GetOrdinal("CloseDate")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                            });
                        }
                    }
                }

                return positions;
            });
        }
        List<Position> IPositionRepository.GetAllOpenPositions()
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var positions = new List<Position>();

                using (var cmd = new SqlCommand(
                    "SELECT p.Id, p.OpenDate, p.CloseDate, p.Status, p.InstrumentId, " +
                    "i.InstrumentName, i.Currency, i.ConId " +
                    "FROM [dbo].[Positions] p " +
                    "INNER JOIN [dbo].[Instruments] i ON p.InstrumentId = i.Id " +
                    "WHERE p.CloseDate IS NULL " +
                    "ORDER BY p.OpenDate DESC", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            positions.Add(new Position
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                InstrumentId = reader.GetInt32(reader.GetOrdinal("InstrumentId")),
                                OpenDate = reader.GetDateTime(reader.GetOrdinal("OpenDate")),
                                CloseDate = reader.IsDBNull(reader.GetOrdinal("CloseDate")) ? (DateTime?)null : reader. GetDateTime(reader.GetOrdinal("CloseDate")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                                Instrument = new Instrument
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("InstrumentId")),
                                    InstrumentName = reader.GetString(reader.GetOrdinal("InstrumentName")),
                                    Currency = reader.GetString(reader.GetOrdinal("Currency")),
                                    ConId = reader.GetString(reader.GetOrdinal("ConId"))
                                }
                            });
                        }
                    }
                }

                return positions;
            });
        }

        /// <summary>
        /// Inserts or updates positions in the database
        /// </summary>
        void IPositionRepository.UpsertPositions(List<Position> positions)
        {
            if (positions == null || !positions.Any())
            {
                Console.WriteLine("No positions to upsert.");
                return;
            }

            ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    int insertedCount = 0;
                    int updatedCount = 0;

                    foreach (var position in positions)
                    {
                        // Ensure instrument exists before upserting position
                        if (position.InstrumentId == 0)
                        {
                            Console.WriteLine($"Position for {position.Id} missing InstrumentId. Skipping.");
                            continue;
                        }

                        // Check if position already exists for the same InstrumentId and OpenDate
                        bool exists = RecordExists(connection, transaction,
                            "SELECT COUNT(*) FROM dbo.Positions WHERE InstrumentId = @instrumentId AND CAST(OpenDate AS DATE) = CAST(@openDate AS DATE)",
                            new Dictionary<string, object>
                            {
                                { "@instrumentId", position.InstrumentId },
                                { "@openDate", position.OpenDate }
                            });

                        if (exists)
                        {
                            // Update existing position
                            string updateQuery = @"
                                UPDATE [dbo].[Positions]
                                SET Status = @status
                                WHERE InstrumentId = @instrumentId 
                                AND CAST(OpenDate AS DATE) = CAST(@openDate AS DATE)";

                            var updateParameters = new Dictionary<string, object>
                            {
                                { "@status", position.Status },
                                { "@instrumentId", position.InstrumentId },
                                { "@openDate", position.OpenDate }
                            };

                            ExecuteCommand(connection, transaction, updateQuery, updateParameters);
                            updatedCount++;
                        }
                        else
                        {
                            // Insert new position
                            string insertQuery = @"
                                INSERT INTO [dbo].[Positions] (OpenDate, Status, InstrumentId)
                                VALUES (@openDate, @status, @instrumentId)";

                            var insertParameters = new Dictionary<string, object>
                            {
                                { "@openDate", position.OpenDate },
                                { "@status", position.Status },
                                { "@instrumentId", position.InstrumentId }
                            };

                            ExecuteCommand(connection, transaction, insertQuery, insertParameters);
                            insertedCount++;
                        }
                    }
                    transaction.Commit();

                    Console.WriteLine($"Successfully processed {positions.Count} positions: {insertedCount} inserted, {updatedCount} updated.");
                }
            });
        }

        /// <summary>
        /// Gets an open position by symbol and instrument ID
        /// </summary>
        Position? IPositionRepository.GetOpenPosition(string symbol, int instrumentId)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                return ((IPositionRepository)this).GetOpenPosition(connection, null, symbol, instrumentId);
            });
        }

        /// <summary>
        /// Gets an open position by symbol and instrument ID within a transaction
        /// </summary>
        Position? IPositionRepository.GetOpenPosition(SqlConnection connection, SqlTransaction transaction, string symbol, int instrumentId)
        {
            const string query = @"
                SELECT p.Id, p.InstrumentId, p.OpenDate, p.Status
                FROM [dbo].[Positions] p WITH (UPDLOCK, ROWLOCK)
                WHERE p.InstrumentId = @instrumentId
                AND p.Status = 'Open'";

            var parameters = new Dictionary<string, object>
            {
                { "@instrumentId", instrumentId }
            };

            using (var cmd = new SqlCommand(query, connection, transaction))
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Position
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            InstrumentId = reader.GetInt32(reader.GetOrdinal("InstrumentId")),
                            OpenDate = reader.GetDateTime(reader.GetOrdinal("OpenDate")),
                            Status = reader.GetString(reader.GetOrdinal("Status"))
                        };
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a new position and returns its ID
        /// </summary>
        int IPositionRepository.CreatePosition(int instrumentId, string symbol, DateTime openDate)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    int positionId = ((IPositionRepository)this).CreatePosition(connection, transaction, instrumentId, symbol, openDate);
                    transaction.Commit();
                    return positionId;
                }
            });
        }

        /// <summary>
        /// Creates a new position and returns its ID within a transaction
        /// </summary>
        int IPositionRepository.CreatePosition(SqlConnection connection, SqlTransaction transaction, int instrumentId, string symbol, DateTime openDate)
        {
            const string insertQuery = @"
                INSERT INTO [dbo].[Positions] (OpenDate, Status, InstrumentId)
                VALUES (@openDate, @status, @instrumentId);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var parameters = new Dictionary<string, object>
            {
                { "@openDate", openDate },
                { "@status", "Open" },
                { "@instrumentId", instrumentId }
            };

            using (var cmd = new SqlCommand(insertQuery, connection, transaction))
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }

                var result = cmd.ExecuteScalar();
                int newPositionId = Convert.ToInt32(result);

                Console.WriteLine($"Created new Position (Id: {newPositionId}) for symbol {symbol}, InstrumentId {instrumentId} on {openDate:yyyy-MM-dd}");

                return newPositionId;
            }
        }

        /// <summary>
        /// Closes a position by setting its status to 'Closed' and close date
        /// </summary>
        void IPositionRepository.ClosePosition(SqlConnection connection, SqlTransaction transaction, int positionId, DateTime closeDate)
        {
            const string updateQuery = @"
                UPDATE [dbo].[Positions]
                SET Status = 'Closed', CloseDate = @closeDate
                WHERE Id = @positionId";

            var parameters = new Dictionary<string, object>
            {
                { "@positionId", positionId },
                { "@closeDate", closeDate }
            };

            using (var cmd = new SqlCommand(updateQuery, connection, transaction))
            {
                try
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value);
                    }

                    cmd.ExecuteNonQuery();
                    Console.WriteLine($"Closed Position (Id: {positionId}) on {closeDate:yyyy-MM-dd}");
                }
                catch
                {
                    Console.WriteLine("Error closing position with Id: {positionId}. Please check if the position exists and is open.");
                }
            }
        }
    }
}