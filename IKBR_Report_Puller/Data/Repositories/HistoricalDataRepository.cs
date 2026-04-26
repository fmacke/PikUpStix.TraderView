using System;
using System.Collections.Generic;
using System.Linq;
using IKBR_Report_Puller.Domain;
using IKBR_Report_Puller.Interfaces;
using Microsoft.Data.SqlClient;

namespace IKBR_Report_Puller.Data.Repositories
{
    /// <summary>
    /// Repository for HistoricalData (chart data) operations
    /// </summary>
    public class HistoricalDataRepository : BaseRepository, IHistoricalDataRepository
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
                    // Use bulk insert for better performance
                    InsertHistoricalData(connection, transaction, instrumentIdInt, newBars);
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
                    // Ensure endDate is after startDate for single-day ranges
                    var adjustedEndDate = startDate == endDate ? endDate.AddDays(1) : endDate;
                    return new List<(DateTime, DateTime)> { (startDate, adjustedEndDate) };
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
                    // No missing dates found
                    return missingRanges;
                }

                // Group consecutive missing dates into ranges
                DateTime? rangeStart = null;
                DateTime? rangeEnd = null;

                foreach (var date in missingDates)
                {
                    if (rangeStart == null)
                    {
                        // Start a new range
                        rangeStart = date;
                        rangeEnd = date;
                    }
                    else
                    {
                        // Check if this date is consecutive to the current range
                        var daysDiff = (date - rangeEnd.Value).Days;
                        bool isConsecutive = daysDiff == 1;

                        // Also consider weekends: Monday following Friday is consecutive
                        bool isWeekendGap = date.DayOfWeek == DayOfWeek.Monday && 
                                          rangeEnd.Value.DayOfWeek == DayOfWeek.Friday && 
                                          daysDiff <= 3;

                        if (isConsecutive || isWeekendGap)
                        {
                            // Extend the current range
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
                }

                // Add the final range
                if (rangeStart.HasValue && rangeEnd.HasValue)
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

        private void InsertHistoricalData(SqlConnection connection, SqlTransaction transaction, int instrumentId, List<Bar> bars)
        {
            if (bars == null || !bars.Any())
            {
                return;
            }

            // Use parameterized batch insert for better performance
            const string insertQuery = @"
                INSERT INTO [dbo].[HistoricalData]
                ([Date], [OpenPrice], [ClosePrice], [LowPrice], [HighPrice], [Volume], [Settle], [OpenInterest], [InstrumentId])
                VALUES (@date, @openPrice, @closePrice, @lowPrice, @highPrice, @volume, @settle, @openInterest, @instrumentId)";

            using (var cmd = new SqlCommand(insertQuery, connection, transaction))
            {
                // Add parameters once with explicit precision and scale for Decimal types
                cmd.Parameters.Add("@date", System.Data.SqlDbType.DateTime);

                // Decimal parameters require explicit Precision and Scale for Prepare()
                // Using 18,6 which allows for large numbers with reasonable precision
                var openPriceParam = cmd.Parameters.Add("@openPrice", System.Data.SqlDbType.Decimal);
                openPriceParam.Precision = 18;
                openPriceParam.Scale = 6;

                var closePriceParam = cmd.Parameters.Add("@closePrice", System.Data.SqlDbType.Decimal);
                closePriceParam.Precision = 18;
                closePriceParam.Scale = 6;

                var lowPriceParam = cmd.Parameters.Add("@lowPrice", System.Data.SqlDbType.Decimal);
                lowPriceParam.Precision = 18;
                lowPriceParam.Scale = 6;

                var highPriceParam = cmd.Parameters.Add("@highPrice", System.Data.SqlDbType.Decimal);
                highPriceParam.Precision = 18;
                highPriceParam.Scale = 6;

                var volumeParam = cmd.Parameters.Add("@volume", System.Data.SqlDbType.Decimal);
                volumeParam.Precision = 18;
                volumeParam.Scale = 6;

                var settleParam = cmd.Parameters.Add("@settle", System.Data.SqlDbType.Decimal);
                settleParam.Precision = 18;
                settleParam.Scale = 6;

                var openInterestParam = cmd.Parameters.Add("@openInterest", System.Data.SqlDbType.Decimal);
                openInterestParam.Precision = 18;
                openInterestParam.Scale = 6;

                cmd.Parameters.Add("@instrumentId", System.Data.SqlDbType.Int);

                // Prepare the command once
                try
                {
                    cmd.Prepare();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error preparing SQL command: {ex.Message}");
                    throw;
                }

                // Execute for each bar
                foreach (var bar in bars)
                {
                    cmd.Parameters["@date"].Value = bar.Date;
                    cmd.Parameters["@openPrice"].Value = bar.OpenPrice;
                    cmd.Parameters["@closePrice"].Value = bar.ClosePrice;
                    cmd.Parameters["@lowPrice"].Value = bar.LowPrice;
                    cmd.Parameters["@highPrice"].Value = bar.HighPrice;
                    cmd.Parameters["@volume"].Value = bar.Volume;
                    cmd.Parameters["@settle"].Value = bar.Settle;
                    cmd.Parameters["@openInterest"].Value = bar.OpenInterest;
                    cmd.Parameters["@instrumentId"].Value = instrumentId;

                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion
    }
}
