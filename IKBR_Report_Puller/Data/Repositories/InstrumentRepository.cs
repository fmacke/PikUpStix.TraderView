using System;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Math;
using IKBR_Report_Puller.Domain;
using Microsoft.Data.SqlClient;
using PikUpStix.TraderView.Interfaces;

namespace IKBR_Report_Puller.Data.Repositories
{
    /// <summary>
    /// Repository for Instrument-related database operations
    /// </summary>
    public class InstrumentRepository : BaseRepository, IInstrumentRepository
    {
        public InstrumentRepository(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Ensures instruments exist for the given trades
        /// Creates missing instruments automatically
        /// </summary>
        public void UpsertInstruments(List<TradeExecution> trades, string source)
        {
            if (trades == null || !trades.Any())
                return;

            ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var uniqueConids = trades
                            .Where(t => !string.IsNullOrEmpty(t.Conid))
                            .Select(t => t.Conid)
                            .Distinct()
                            .ToList();

                        int createdCount = 0;
                        int existingCount = 0;

                        foreach (var conid in uniqueConids)
                        {
                            int? instrumentId = GetInstrumentIdByConid(connection, transaction, conid);

                            if (!instrumentId.HasValue)
                            {
                                var trade = trades.First(t => t.Conid == conid);

                                InsertInstrument(
                                    connection,
                                    transaction,
                                    conid,
                                    trade.Symbol,
                                    trade.ListingExchange,
                                    trade.Currency,
                                    trade.AssetCategory,
                                    source,
                                    source);

                                createdCount++;
                            }
                            else
                            {
                                existingCount++;
                            }
                        }

                        transaction.Commit();

                        if (createdCount > 0)
                        {
                            Console.WriteLine($"Created {createdCount} new instrument(s), {existingCount} already existed");
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error upserting instruments: {ex.Message}");
                        throw;
                    }
                }
                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var trade in trades.Where(x => x.InstrumentId == 0))
                    {
                        if (!string.IsNullOrEmpty(trade.Conid))
                        {
                            int? instrumentId = GetInstrumentIdByConid(connection, transaction, trade.Conid);
                            if (instrumentId.HasValue)
                            {
                                trade.InstrumentId = instrumentId.Value;
                            }
                        }
                    }
                }
            });

            
        }

        

        /// <summary>
        /// Ensures instruments exist for the given trade confirmations
        /// Creates missing instruments automatically and populates InstrumentID on each trade confirm
        /// </summary>
        //public void UpsertInstruments(List<TradeExecution> tradeConfirms, string source)
        //{
        //    if (tradeConfirms == null || !tradeConfirms.Any())
        //        return;

        //    ExecuteDatabaseOperation(connection =>
        //    {
        //        using (var transaction = connection.BeginTransaction())
        //        {
        //            try
        //            {
        //                var uniqueConids = tradeConfirms
        //                    .Where(t => !string.IsNullOrEmpty(t.Conid))
        //                    .Select(t => t.Conid)
        //                    .Distinct()
        //                    .ToList();

        //                int createdCount = 0;
        //                int existingCount = 0;

        //                // Dictionary to cache conid -> instrumentId mappings
        //                var conidToInstrumentIdMap = new Dictionary<string, int>();

        //                foreach (var conid in uniqueConids)
        //                {
        //                    int? instrumentId = GetInstrumentIdByConid(connection, transaction, conid);

        //                    if (!instrumentId.HasValue)
        //                    {
        //                        var tradeConfirm = tradeConfirms.First(t => t.Conid == conid);

        //                        InsertInstrument(
        //                            connection,
        //                            transaction,
        //                            conid,
        //                            tradeConfirm.Symbol,
        //                            tradeConfirm.ListingExchange,
        //                            tradeConfirm.Currency,
        //                            tradeConfirm.AssetCategory,
        //                            source,
        //                            source);

        //                        // Get the newly created instrument ID
        //                        instrumentId = GetInstrumentIdByConid(connection, transaction, conid);
        //                        createdCount++;
        //                    }
        //                    else
        //                    {
        //                        existingCount++;
        //                    }

        //                    // Store the mapping
        //                    if (instrumentId.HasValue)
        //                    {
        //                        conidToInstrumentIdMap[conid] = instrumentId.Value;
        //                    }
        //                }

        //                // Populate InstrumentID on all trade confirms
        //                foreach (var tradeConfirm in tradeConfirms)
        //                {
        //                    if (!string.IsNullOrEmpty(tradeConfirm.Conid) && 
        //                        conidToInstrumentIdMap.TryGetValue(tradeConfirm.Conid, out int instrumentId))
        //                    {
        //                        //tradeConfirm.InstrumentID = instrumentId.ToString();
        //                    }
        //                }

        //                transaction.Commit();

        //                if (createdCount > 0)
        //                {
        //                    Console.WriteLine($"Created {createdCount} new instrument(s) from trade confirmations, {existingCount} already existed");
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                transaction.Rollback();
        //                Console.WriteLine($"Error upserting instruments from trade confirmations: {ex.Message}");
        //                throw;
        //            }
        //        }
        //    });
        //}

        #region Private Helper Methods
        public Instrument Get(int instrumentId)
        {
            Instrument instrument = null;
            ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        const string query = @"
                            SELECT Id, InstrumentName, Provider, DataName, DataSource, Format, Frequency, 
                                   ContractUnit, ContractUnitType, PriceQuotation, MinimumPriceFluctuation, 
                                   Currency, ListingExchange, ConId 
                            FROM dbo.Instruments 
                            WHERE Id = @instrumentId";

                        var parameters = new Dictionary<string, object>
                        {
                            { "@instrumentId", instrumentId }
                        };

                        using (var cmd = new SqlCommand(query, connection, transaction))
                        {
                            foreach (var param in parameters)
                            {
                                cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                            }

                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    instrument = new Instrument
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                        InstrumentName = reader.IsDBNull(reader.GetOrdinal("InstrumentName")) 
                                            ? null : reader.GetString(reader.GetOrdinal("InstrumentName")),
                                        Provider = reader.IsDBNull(reader.GetOrdinal("Provider")) 
                                            ? null : reader.GetString(reader.GetOrdinal("Provider")),
                                        DataName = reader.IsDBNull(reader.GetOrdinal("DataName")) 
                                            ? null : reader.GetString(reader.GetOrdinal("DataName")),
                                        DataSource = reader.IsDBNull(reader.GetOrdinal("DataSource")) 
                                            ? null : reader.GetString(reader.GetOrdinal("DataSource")),
                                        Format = reader.IsDBNull(reader.GetOrdinal("Format")) 
                                            ? null : reader.GetString(reader.GetOrdinal("Format")),
                                        Frequency = reader.IsDBNull(reader.GetOrdinal("Frequency")) 
                                            ? null : reader.GetString(reader.GetOrdinal("Frequency")),
                                        ContractUnit = reader.IsDBNull(reader.GetOrdinal("ContractUnit")) 
                                            ? null : reader.GetDouble(reader.GetOrdinal("ContractUnit")),
                                        ContractUnitType = reader.IsDBNull(reader.GetOrdinal("ContractUnitType")) 
                                            ? null : reader.GetString(reader.GetOrdinal("ContractUnitType")),
                                        PriceQuotation = reader.IsDBNull(reader.GetOrdinal("PriceQuotation")) 
                                            ? null : reader.GetString(reader.GetOrdinal("PriceQuotation")),
                                        MinimumPriceFluctuation = reader.IsDBNull(reader.GetOrdinal("MinimumPriceFluctuation")) 
                                            ? null : reader.GetDouble(reader.GetOrdinal("MinimumPriceFluctuation")),
                                        Currency = reader.IsDBNull(reader.GetOrdinal("Currency")) 
                                            ? null : reader.GetString(reader.GetOrdinal("Currency")),
                                        ListingExchange = reader.IsDBNull(reader.GetOrdinal("ListingExchange")) 
                                            ? null : reader.GetString(reader.GetOrdinal("ListingExchange")),
                                        ConId = reader.IsDBNull(reader.GetOrdinal("ConId")) 
                                            ? null : reader.GetString(reader.GetOrdinal("ConId"))
                                    };
                                }
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error retrieving instrument by ID: {ex.Message}");
                        throw;
                    }
                }
            });
            return instrument;
        }

        public int? GetInstrumentIdFromConId(string conid)
        {
            int? instrumentId = null;
            ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        instrumentId = GetInstrumentIdByConid(connection, transaction, conid);

                        return instrumentId;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error obtaining instrumentId from conid: {ex.Message}");
                        throw;
                    }
                }
            });
            return instrumentId;
        }

        private int? GetInstrumentIdByConid(SqlConnection connection, SqlTransaction transaction, string conid)
        {
            const string query = "SELECT Id FROM dbo.Instruments WHERE ConId = @conid";

            var parameters = new Dictionary<string, object>
            {
                { "@conid", conid }
            };

            int instrumentId = ExecuteScalar<int>(connection, transaction, query, parameters);
            return instrumentId > 0 ? instrumentId : (int?)null;
        }
        public int? GetInstrumentIdFromSymbol(string symbol, string provider)
        {
            int? instrumentId = null;
            ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        instrumentId = GetInstrumentIdBySymbol(connection, transaction, symbol, provider);

                        return instrumentId;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error obtaining instrumentId from symbol: {ex.Message}");
                        throw;
                    }
                }
            });
            return instrumentId;
        }

        private int? GetInstrumentIdBySymbol(SqlConnection connection, SqlTransaction transaction, string symbol, string provider)
        {
            const string query = "SELECT Id FROM dbo.Instruments WHERE Symbol = @symbol AND Provider = @provider";

            var parameters = new Dictionary<string, object>
            {
                { "@symbol", symbol },
                { "@provider", provider }
            };

            int instrumentId = ExecuteScalar<int>(connection, transaction, query, parameters);
            return instrumentId > 0 ? instrumentId : (int?)null;
        }

        public int? InsertInstrument(string conid, string symbol, string listingExchange, string currency, string assetCategory, string provider, string dataSource)
        {
            int? id = null;
            ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        InsertInstrument(
                                    connection,
                                    transaction,
                                    conid,
                                    symbol,
                                    listingExchange,
                                    currency,
                                    assetCategory,
                                    provider,
                                    dataSource);

                        // Get the newly created instrument ID
                        id = GetInstrumentIdByConid(connection, transaction, conid);
                        transaction.Commit();
                        return id;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error upserting instruments from trade confirmations: {ex.Message}");
                        throw;
                    }
                }
            });
            return id;
        }
        private void InsertInstrument(
            SqlConnection connection,
            SqlTransaction transaction,
            string conid,
            string symbol,
            string listingExchange,
            string currency,
            string assetCategory,
            string provider,
            string dataSource)
        {
            const string insertQuery = @"
                INSERT INTO dbo.Instruments 
                (InstrumentName, Provider, DataName, DataSource, Format, Frequency, ContractUnit, ContractUnitType, 
                 PriceQuotation, MinimumPriceFluctuation, Currency, ListingExchange, ConId) 
                VALUES 
                (@instrumentName, @provider, @dataName, @dataSource, @format, @frequency, @contractUnit, @contractUnitType, 
                 @priceQuotation, @minimumPriceFluctuation, @currency, @listingExchange, @conId)";

            var parameters = new Dictionary<string, object>
            {
                { "@instrumentName", symbol ?? "Unknown" },
                { "@provider", provider ?? "Unknown" },
                { "@dataName", symbol ?? "Unknown" },
                { "@dataSource", dataSource ?? "Unknown" },
                { "@format", "TradeExecution" },
                { "@frequency", "TradeExecution" },
                { "@contractUnit", DBNull.Value },
                { "@contractUnitType", assetCategory },
                { "@priceQuotation", DBNull.Value },
                { "@minimumPriceFluctuation", DBNull.Value },
                { "@currency", (object)currency ?? DBNull.Value },
                { "@listingExchange", (object)listingExchange ?? DBNull.Value },
                { "@conId", conid }
            };

            ExecuteCommand(connection, transaction, insertQuery, parameters);
        }

        /// <summary>
        /// Gets an instrument by its ID asynchronously
        /// </summary>
        public async Task<Instrument?> GetByIdAsync(int instrumentId)
        {
            return await Task.Run(() =>
            {
                Instrument? instrument = null;
                ExecuteDatabaseOperation(connection =>
                {
                    const string query = @"
                        SELECT Id, InstrumentName, Provider, DataName, Currency, ListingExchange
                        FROM dbo.Instruments
                        WHERE Id = @InstrumentId";

                    using var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@InstrumentId", instrumentId);

                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        instrument = new Instrument
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            InstrumentName = reader.IsDBNull(reader.GetOrdinal("InstrumentName"))
                                ? string.Empty : reader.GetString(reader.GetOrdinal("InstrumentName")),
                            Provider = reader.IsDBNull(reader.GetOrdinal("Provider"))
                                ? string.Empty : reader.GetString(reader.GetOrdinal("Provider")),
                            DataName = reader.IsDBNull(reader.GetOrdinal("DataName"))
                                ? string.Empty : reader.GetString(reader.GetOrdinal("DataName")),
                            Currency = reader.IsDBNull(reader.GetOrdinal("Currency"))
                                ? string.Empty : reader.GetString(reader.GetOrdinal("Currency")),
                            ListingExchange = reader.IsDBNull(reader.GetOrdinal("ListingExchange"))
                                ? string.Empty : reader.GetString(reader.GetOrdinal("ListingExchange"))
                        };
                    }
                });
                return instrument;
            });
        }
        #endregion
    }
}
