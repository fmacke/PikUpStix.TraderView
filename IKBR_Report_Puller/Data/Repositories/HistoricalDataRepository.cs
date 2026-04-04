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
        public void InsertChartData(string instrumentId, List<Bar> bars)
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
                        InsertBar(connection, transaction, instrumentIdInt, bar);
                    }
                    transaction.Commit();
                    Console.WriteLine($"Successfully inserted {newBars.Count} new chart data records for instrument {instrumentId}.");
                }
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

        private void InsertBar(SqlConnection connection, SqlTransaction transaction, int instrumentId, Bar bar)
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
