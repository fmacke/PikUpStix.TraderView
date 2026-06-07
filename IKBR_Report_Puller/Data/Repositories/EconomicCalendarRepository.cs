using IKBR_Report_Puller.Domain;
using Microsoft.Data.SqlClient;
using PikUpStix.TraderView.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;

namespace IKBR_Report_Puller.Data.Repositories
{
    /// <summary>
    /// Repository for economic calendar database operations
    /// </summary>
    public class EconomicCalendarRepository : BaseRepository, IEconomicCalendarRepository
    {
        public EconomicCalendarRepository(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Inserts or updates economic calendar events using MERGE statement
        /// </summary>
        public void UpsertEconomicCalendarEvents(List<EconomicCalendarEvent> events)
        {
            if (events == null || events.Count == 0)
            {
                Console.WriteLine("No economic calendar events to insert.");
                return;
            }

            ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var evt in events)
                        {
                            using (var cmd = new SqlCommand(@"
                                MERGE dbo.EconomicCalendar AS target
                                USING (SELECT @Date, @Country, @Event, @Currency, @Previous, @Estimate, @Actual, @Change, @Impact, @ChangePercentage, @Unit) 
                                    AS source (Date, Country, Event, Currency, Previous, Estimate, Actual, Change, Impact, ChangePercentage, Unit)
                                ON target.Date = source.Date 
                                    AND target.Country = source.Country 
                                    AND target.Event = source.Event
                                WHEN MATCHED THEN
                                    UPDATE SET 
                                        Currency = source.Currency,
                                        Previous = source.Previous,
                                        Estimate = source.Estimate,
                                        Actual = source.Actual,
                                        Change = source.Change,
                                        Impact = source.Impact,
                                        ChangePercentage = source.ChangePercentage,
                                        Unit = source.Unit,
                                        UpdatedAt = GETUTCDATE()
                                WHEN NOT MATCHED THEN
                                    INSERT (Date, Country, Event, Currency, Previous, Estimate, Actual, Change, Impact, ChangePercentage, Unit, CreatedAt, UpdatedAt)
                                    VALUES (source.Date, source.Country, source.Event, source.Currency, source.Previous, source.Estimate, source.Actual, source.Change, source.Impact, source.ChangePercentage, source.Unit, GETUTCDATE(), GETUTCDATE());",
                                connection, transaction))
                            {
                                cmd.Parameters.Add("@Date", SqlDbType.DateTime2).Value = evt.Date;
                                cmd.Parameters.Add("@Country", SqlDbType.NVarChar, 50).Value = evt.Country ?? (object)DBNull.Value;
                                cmd.Parameters.Add("@Event", SqlDbType.NVarChar, 500).Value = evt.Event ?? (object)DBNull.Value;
                                cmd.Parameters.Add("@Currency", SqlDbType.NVarChar, 10).Value = evt.Currency ?? (object)DBNull.Value;
                                cmd.Parameters.Add("@Previous", SqlDbType.Decimal).Value = evt.Previous.HasValue ? (object)evt.Previous.Value : DBNull.Value;
                                cmd.Parameters.Add("@Estimate", SqlDbType.Decimal).Value = evt.Estimate.HasValue ? (object)evt.Estimate.Value : DBNull.Value;
                                cmd.Parameters.Add("@Actual", SqlDbType.Decimal).Value = evt.Actual.HasValue ? (object)evt.Actual.Value : DBNull.Value;
                                cmd.Parameters.Add("@Change", SqlDbType.Decimal).Value = evt.Change.HasValue ? (object)evt.Change.Value : DBNull.Value;
                                cmd.Parameters.Add("@Impact", SqlDbType.NVarChar, 50).Value = evt.Impact ?? (object)DBNull.Value;
                                cmd.Parameters.Add("@ChangePercentage", SqlDbType.Decimal).Value = evt.ChangePercentage.HasValue ? (object)evt.ChangePercentage.Value : DBNull.Value;
                                cmd.Parameters.Add("@Unit", SqlDbType.NVarChar, 50).Value = evt.Unit ?? (object)DBNull.Value;

                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        Console.WriteLine($"Successfully upserted {events.Count} economic calendar events.");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error upserting economic calendar events: {ex.Message}");
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// Retrieves all economic calendar events from the database
        /// </summary>
        public List<EconomicCalendarEvent> GetAllEvents()
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var events = new List<EconomicCalendarEvent>();

                using (var cmd = new SqlCommand(@"
                    SELECT Date, Country, Event, Currency, Previous, Estimate, Actual, Change, Impact, ChangePercentage, Unit
                    FROM dbo.EconomicCalendar
                    ORDER BY Date DESC", connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            events.Add(new EconomicCalendarEvent
                            {
                                Date = reader.GetDateTime(0),
                                Country = reader.IsDBNull(1) ? null : reader.GetString(1),
                                Event = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Currency = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Previous = reader.IsDBNull(4) ? null : reader.GetDecimal(4),
                                Estimate = reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                                Actual = reader.IsDBNull(6) ? null : reader.GetDecimal(6),
                                Change = reader.IsDBNull(7) ? null : reader.GetDecimal(7),
                                Impact = reader.IsDBNull(8) ? null : reader.GetString(8),
                                ChangePercentage = reader.IsDBNull(9) ? null : reader.GetDecimal(9),
                                Unit = reader.IsDBNull(10) ? null : reader.GetString(10)
                            });
                        }
                    }
                }

                Console.WriteLine($"Retrieved {events.Count} economic calendar events.");
                return events;
            });
        }
    }
}
