using System;
using System.Collections.Generic;
using System.Linq;
using IKBR_Report_Puller.Domain;
using Microsoft.Data.SqlClient;

namespace IKBR_Report_Puller.Data.Repositories
{
    /// <summary>
    /// Repository for HistoricalData (chart data) operations
    /// </summary>
    public class HistoricalDataRepository : BaseRepository
    {
        public HistoricalDataRepository(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Inserts chart data bars for a given instrument, skipping duplicates
        /// </summary>
        public void UpdateHistoricalData(string instrumentId, List<Bar> bars)
        {
            if (bars == null || !bars.Any())
            {
                Console.WriteLine("No bars data to insert.");
                return;
            }

            if (!int.TryParse(instrumentId, out int instrumentIdInt))
            {
                Console.WriteLine($"Invalid instrument ID: {instrumentId}");
                return;
            }

            ExecuteDatabaseOperation(connection =>
            {
                var existingDates = GetExistingDates(connection, instrumentIdInt);
                var newBars = bars.Where(bar => !existingDates.Contains(bar.Date)).ToList();

                if (!newBars.Any())
                {
                    Console.WriteLine($"All chart data already exists for instrument {instrumentId}.");
                    return;
                }

                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var bar in newBars)
                    {
                        InsertHistoricalData(connection, transaction, instrumentIdInt, bar);
                    }
                    transaction.Commit();
                    Console.WriteLine($"Successfully inserted {newBars.Count} new chart data records for instrument {instrumentId}.");
                }
            });
        }

        /// <summary>
        /// Gets missing date ranges for historical data for a given instrument and date range
        /// </summary>
        public List<(DateTime startDate, DateTime endDate)> GetMissingDateRanges(int instrumentId, DateTime startDate, DateTime endDate)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var existingDates = GetExistingDates(connection, instrumentId);
                var missingRanges = new List<(DateTime startDate, DateTime endDate)>();

                if (!existingDates.Any())
                {
                    // No data exists, return the entire range
                    return new List<(DateTime, DateTime)> { (startDate, endDate) };
                }

                // Generate all expected dates (trading days approximation - all weekdays)
                var expectedDates = new List<DateTime>();
                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    // Skip weekends (rough approximation - doesn't account for holidays)
                    if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                    {
                        expectedDates.Add(date.Date);
                    }
                }

                // Find missing dates
                var missingDates = expectedDates.Where(d => !existingDates.Contains(d.Date)).OrderBy(d => d).ToList();

                if (!missingDates.Any())
                {
                    return missingRanges;
                }

                // Group consecutive missing dates into ranges
                DateTime? rangeStart = null;
                DateTime? rangeEnd = null;

                foreach (var date in missingDates)
                {
                    if (rangeStart == null)
                    {
                        rangeStart = date;
                        rangeEnd = date;
                    }
                    else if (date == rangeEnd.Value.AddDays(1) || 
                             (date.DayOfWeek == DayOfWeek.Monday && rangeEnd.Value.DayOfWeek == DayOfWeek.Friday && (date - rangeEnd.Value).Days <= 3))
                    {
                        // Consecutive date (or Monday following Friday)
                        rangeEnd = date;
                    }
                    else
                    {
                        // Gap detected, save current range and start new one
                        missingRanges.Add((rangeStart.Value, rangeEnd.Value));
                        rangeStart = date;
                        rangeEnd = date;
                    }
                }

                // Add the last range
                if (rangeStart != null)
                {
                    missingRanges.Add((rangeStart.Value, rangeEnd.Value));
                }

                return missingRanges;
            });
        }

        #region Private Helper Methods

        private HashSet<DateTime> GetExistingDates(SqlConnection connection, int instrumentId)
        {
            var existingDates = new HashSet<DateTime>();
            using (var cmd = new SqlCommand("SELECT [Date] FROM dbo.HistoricalData WHERE InstrumentId = @instrumentId", connection))
            {
                cmd.Parameters.AddWithValue("@instrumentId", instrumentId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        existingDates.Add(reader.GetDateTime(0));
                    }
                }
            }
            return existingDates;
        }

        private void InsertHistoricalData(SqlConnection connection, SqlTransaction transaction, int instrumentId, Bar bar)
        {
            const string insertQuery = @"
                INSERT INTO [dbo].[HistoricalData]
                ([Date], [OpenPrice], [ClosePrice], [LowPrice], [HighPrice], [Volume], [Settle], [OpenInterest], [InstrumentId])
                VALUES (@date, @openPrice, @closePrice, @lowPrice, @highPrice, @volume, @settle, @openInterest, @instrumentId)";

            var parameters = new Dictionary<string, object>
            {
                { "@date", bar.Date },
                { "@openPrice", bar.OpenPrice },
                { "@closePrice", bar.ClosePrice },
                { "@lowPrice", bar.LowPrice },
                { "@highPrice", bar.HighPrice },
                { "@volume", bar.Volume },
                { "@settle", bar.Settle },
                { "@openInterest", bar.OpenInterest },
                { "@instrumentId", instrumentId }
            };

            ExecuteCommand(connection, transaction, insertQuery, parameters);
        }

        #endregion
    }
}
