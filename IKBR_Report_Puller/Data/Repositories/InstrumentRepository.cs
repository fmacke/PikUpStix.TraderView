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

                // Additional time series data insertion logic can be added here
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

        #endregion
    }
}
