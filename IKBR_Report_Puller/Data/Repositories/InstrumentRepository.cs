using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace IKBR_Report_Puller.Data.Repositories
{
    /// <summary>
    /// Repository for Instrument-related database operations
    /// </summary>
    public class InstrumentRepository : BaseRepository
    {
        public InstrumentRepository(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Gets or creates an instrument by conid and returns its InstrumentId
        /// </summary>
        public int GetOrCreateInstrumentByConid(
            string conid,
            string symbol,
            string listingExchange,
            string currency,
            string assetCategory,
            string securityID,
            string description)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                int? instrumentId = GetInstrumentIdByConid(connection, null, conid);

                if (instrumentId.HasValue)
                {
                    return instrumentId.Value;
                }

                // Instrument doesn't exist, create it
                InsertInstrumentFromTrade(connection, null, conid, symbol, listingExchange, 
                    currency, assetCategory, securityID, description);

                // Get the newly created instrument ID
                instrumentId = GetInstrumentIdByConid(connection, null, conid);
                return instrumentId ?? 0;
            });
        }

        /// <summary>
        /// Upserts time series instrument data
        /// </summary>
        public void UpsertTimeSeriesData(
            string instrumentName,
            string listingExchange,
            string securityIdentifier,
            string provider,
            string dataName,
            string dataSource,
            string format,
            string frequency,
            string currency,
            DateTime date,
            double openPrice,
            double closePrice,
            double lowPrice,
            double highPrice,
            double volume)
        {
            ExecuteDatabaseOperation(connection =>
            {
                bool instrumentExists = CheckInstrumentExists(connection, securityIdentifier, frequency, provider);

                if (!instrumentExists)
                {
                    InsertInstrument(connection, instrumentName, listingExchange, securityIdentifier, provider,
                        dataName, dataSource, format, frequency, currency);
                }

                // Add time series data to HistoricalData table
                AddTimeSeriesData(connection, securityIdentifier, frequency, provider, date, 
                    openPrice, closePrice, lowPrice, highPrice, volume);
            });
        }

        #region Private Helper Methods

        private bool CheckInstrumentExists(SqlConnection connection, string securityId, string frequency, string provider)
        {
            const string query = "SELECT COUNT(*) FROM dbo.Instruments WHERE SecurityId = @securityId AND Frequency = @frequency AND Provider = @provider";

            var parameters = new Dictionary<string, object>
            {
                { "@securityId", securityId },
                { "@frequency", frequency },
                { "@provider", provider }
            };

            int count = ExecuteScalar<int>(connection, null, query, parameters);
            return count > 0;
        }

        private void InsertInstrument(
            SqlConnection connection,
            string instrumentName,
            string listingExchange,
            string securityIdentifier,
            string provider,
            string dataName,
            string dataSource,
            string format,
            string frequency,
            string currency)
        {
            const string insertQuery = @"
                INSERT INTO dbo.Instruments 
                (InstrumentName, Provider, DataName, DataSource, Format, Frequency, ContractUnit, ContractUnitType, 
                 PriceQuotation, MinimumPriceFluctuation, Currency, ListingExchange, SecurityId) 
                VALUES 
                (@instrumentName, @provider, @dataName, @dataSource, @format, @frequency, @contractUnit, @contractUnitType, 
                 @priceQuotation, @minimumPriceFluctuation, @currency, @listingExchange, @securityId)";

            var parameters = new Dictionary<string, object>
            {
                { "@instrumentName", instrumentName },
                { "@provider", provider },
                { "@dataName", dataName },
                { "@dataSource", dataSource },
                { "@format", format },
                { "@frequency", frequency },
                { "@contractUnit", DBNull.Value },
                { "@contractUnitType", DBNull.Value },
                { "@priceQuotation", DBNull.Value },
                { "@minimumPriceFluctuation", DBNull.Value },
                { "@currency", currency },
                { "@listingExchange", listingExchange },
                { "@securityId", securityIdentifier }
            };

            using (var cmd = new SqlCommand(insertQuery, connection))
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
                cmd.ExecuteNonQuery();
            }
        }

        private void AddTimeSeriesData(
            SqlConnection connection,
            string securityIdentifier,
            string frequency,
            string provider,
            DateTime date,
            double openPrice,
            double closePrice,
            double lowPrice,
            double highPrice,
            double volume)
        {
            // Get the InstrumentId from the Instruments table
            const string getInstrumentIdQuery = @"
                SELECT Id FROM dbo.Instruments 
                WHERE SecurityId = @securityId AND Frequency = @frequency AND Provider = @provider";

            var instrumentIdParams = new Dictionary<string, object>
            {
                { "@securityId", securityIdentifier },
                { "@frequency", frequency },
                { "@provider", provider }
            };

            int instrumentId = ExecuteScalar<int>(connection, null, getInstrumentIdQuery, instrumentIdParams);

            if (instrumentId == 0)
            {
                Console.WriteLine($"Warning: Instrument not found for SecurityId={securityIdentifier}, Frequency={frequency}, Provider={provider}");
                return;
            }

            // Check if data already exists for this date
            const string checkExistsQuery = @"
                SELECT COUNT(*) FROM dbo.HistoricalData 
                WHERE InstrumentId = @instrumentId AND Date = @date";

            var checkParams = new Dictionary<string, object>
            {
                { "@instrumentId", instrumentId },
                { "@date", date }
            };

            bool dataExists = RecordExists(connection, null, checkExistsQuery, checkParams);

            if (dataExists)
            {
                return;
            }

            // Insert the historical data
            const string insertQuery = @"
                INSERT INTO dbo.HistoricalData
                ([Date], [OpenPrice], [ClosePrice], [LowPrice], [HighPrice], [Volume], [Settle], [OpenInterest], [InstrumentId])
                VALUES (@date, @openPrice, @closePrice, @lowPrice, @highPrice, @volume, @settle, @openInterest, @instrumentId)";

            var insertParams = new Dictionary<string, object>
            {
                { "@date", date },
                { "@openPrice", openPrice },
                { "@closePrice", closePrice },
                { "@lowPrice", lowPrice },
                { "@highPrice", highPrice },
                { "@volume", volume },
                { "@settle", DBNull.Value },
                { "@openInterest", DBNull.Value },
                { "@instrumentId", instrumentId }
            };

            ExecuteCommand(connection, null, insertQuery, insertParams);
        }

        private int? GetInstrumentIdByConid(SqlConnection connection, SqlTransaction transaction, string conid)
        {
            if (string.IsNullOrEmpty(conid))
            {
                return null;
            }

            const string query = "SELECT Id FROM dbo.Instruments WHERE SecurityId = @conid";

            var parameters = new Dictionary<string, object>
            {
                { "@conid", conid }
            };

            int instrumentId = ExecuteScalar<int>(connection, transaction, query, parameters);
            return instrumentId > 0 ? instrumentId : (int?)null;
        }

        private void InsertInstrumentFromTrade(
            SqlConnection connection,
            SqlTransaction transaction,
            string conid,
            string symbol,
            string listingExchange,
            string currency,
            string assetCategory,
            string securityID,
            string description)
        {
            const string insertQuery = @"
                INSERT INTO dbo.Instruments 
                (InstrumentName, Provider, DataName, DataSource, Format, Frequency, ContractUnit, ContractUnitType, 
                 PriceQuotation, MinimumPriceFluctuation, Currency, ListingExchange, SecurityId) 
                VALUES 
                (@instrumentName, @provider, @dataName, @dataSource, @format, @frequency, @contractUnit, @contractUnitType, 
                 @priceQuotation, @minimumPriceFluctuation, @currency, @listingExchange, @securityId)";

            var parameters = new Dictionary<string, object>
            {
                { "@instrumentName", symbol ?? description ?? "Unknown" },
                { "@provider", "IBKR" },
                { "@dataName", assetCategory ?? "Unknown" },
                { "@dataSource", "Trade Execution" },
                { "@format", "Trade" },
                { "@frequency", "Trade" },
                { "@contractUnit", DBNull.Value },
                { "@contractUnitType", DBNull.Value },
                { "@priceQuotation", DBNull.Value },
                { "@minimumPriceFluctuation", DBNull.Value },
                { "@currency", (object)currency ?? DBNull.Value },
                { "@listingExchange", (object)listingExchange ?? DBNull.Value },
                { "@securityId", conid }
            };

            ExecuteCommand(connection, transaction, insertQuery, parameters);
        }

        #endregion
    }
}
