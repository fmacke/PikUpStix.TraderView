using Microsoft.Data.SqlClient;
using PikUpStix.TraderView.Interfaces;
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
                    "SELECT p.Id, p.OpenDate, p.Status, p.InstrumentId, " +
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
                                InstrumentId = reader.GetInt32(reader.GetOrdinal("InstrumentId")),
                                Symbol = reader.GetString(reader.GetOrdinal("Symbol")),
                                Currency = reader.GetString(reader.GetOrdinal("Currency")),
                                SecurityId = reader.GetString(reader.GetOrdinal("SecurityId")),
                                TradeDate = reader.GetDateTime(reader.GetOrdinal("OpenDate")),
                                IsClosed = reader.GetString(reader.GetOrdinal("Status")).Equals("Closed", StringComparison.OrdinalIgnoreCase)
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
                            Console.WriteLine($"Position for {position.Symbol} missing InstrumentId. Skipping.");
                            continue;
                        }

                        // Check if position already exists for the same InstrumentId and OpenDate
                        bool exists = RecordExists(connection, transaction,
                            "SELECT COUNT(*) FROM dbo.Positions WHERE InstrumentId = @instrumentId AND CAST(OpenDate AS DATE) = CAST(@openDate AS DATE)",
                            new Dictionary<string, object> 
                            { 
                                { "@instrumentId", position.InstrumentId },
                                { "@openDate", position.TradeDate }
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
                                { "@status", position.IsClosed ? "Closed" : "Open" },
                                { "@instrumentId", position.InstrumentId },
                                { "@openDate", position.TradeDate }
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
                                { "@openDate", position.TradeDate },
                                { "@status", position.IsClosed ? "Closed" : "Open" },
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
    }
}