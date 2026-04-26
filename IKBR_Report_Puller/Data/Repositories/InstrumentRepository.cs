using System;
using System.Collections.Generic;
using System.Linq;
using IKBR_Report_Puller.Domain;
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
        /// Ensures instruments exist for the given trades
        /// Creates missing instruments automatically
        /// </summary>
        internal void UpsertInstruments(List<Trade> trades)
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

                                InsertInstrumentFromTrade(
                                    connection,
                                    transaction,
                                    conid,
                                    trade.Symbol,
                                    trade.ListingExchange,
                                    trade.Currency,
                                    trade.AssetCategory,
                                    trade.Description);

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
        internal void UpsertInstruments(List<TradeConfirm> tradeConfirms)
        {
            if (tradeConfirms == null || !tradeConfirms.Any())
                return;

            ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var uniqueConids = tradeConfirms
                            .Where(t => !string.IsNullOrEmpty(t.ConId))
                            .Select(t => t.ConId)
                            .Distinct()
                            .ToList();

                        int createdCount = 0;
                        int existingCount = 0;

                        // Dictionary to cache conid -> instrumentId mappings
                        var conidToInstrumentIdMap = new Dictionary<string, int>();

                        foreach (var conid in uniqueConids)
                        {
                            int? instrumentId = GetInstrumentIdByConid(connection, transaction, conid);

                            if (!instrumentId.HasValue)
                            {
                                var tradeConfirm = tradeConfirms.First(t => t.ConId == conid);

                                InsertInstrumentFromTradeConfirm(
                                    connection,
                                    transaction,
                                    conid,
                                    tradeConfirm.Symbol,
                                    tradeConfirm.ListingExchange,
                                    tradeConfirm.Currency);

                                // Get the newly created instrument ID
                                instrumentId = GetInstrumentIdByConid(connection, transaction, conid);
                                createdCount++;
                            }
                            else
                            {
                                existingCount++;
                            }

                            // Store the mapping
                            if (instrumentId.HasValue)
                            {
                                conidToInstrumentIdMap[conid] = instrumentId.Value;
                            }
                        }

                        // Populate InstrumentID on all trade confirms
                        foreach (var tradeConfirm in tradeConfirms)
                        {
                            if (!string.IsNullOrEmpty(tradeConfirm.ConId) && 
                                conidToInstrumentIdMap.TryGetValue(tradeConfirm.ConId, out int instrumentId))
                            {
                                tradeConfirm.InstrumentID = instrumentId.ToString();
                            }
                        }

                        transaction.Commit();

                        if (createdCount > 0)
                        {
                            Console.WriteLine($"Created {createdCount} new instrument(s) from trade confirmations, {existingCount} already existed");
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error upserting instruments from trade confirmations: {ex.Message}");
                        throw;
                    }
                }
            });
        }

        #region Private Helper Methods
        public int? GetInstrumentIdFromConId(string conid)
        {
            ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int? instrumentId = GetInstrumentIdByConid(connection, transaction, conid);

                        return instrumentId;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error obtaining instrumentId from conid: {ex.Message}");
                        throw;
                    }
                }
            });
            return null;
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
        public int? InsertInstrument(string conid, string symbol, string listingExchange, string currency)
        {
            ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        InsertInstrumentFromTradeConfirm(
                                    connection,
                                    transaction,
                                    conid,
                                    symbol,
                                    listingExchange,
                                    currency);

                        // Get the newly created instrument ID
                        return GetInstrumentIdByConid(connection, transaction, conid);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error upserting instruments from trade confirmations: {ex.Message}");
                        throw;
                    }
                }
            });
            return null;
        }
        private void InsertInstrumentFromTrade(
            SqlConnection connection,
            SqlTransaction transaction,
            string conid,
            string symbol,
            string listingExchange,
            string currency,
            string assetCategory,
            string description)
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
                { "@conId", conid }
            };

            ExecuteCommand(connection, transaction, insertQuery, parameters);
        }

        private void InsertInstrumentFromTradeConfirm(
            SqlConnection connection,
            SqlTransaction transaction,
            string conid,
            string symbol,
            string listingExchange,
            string currency)
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
                { "@provider", "IBKR" },
                { "@dataName", "Trade Confirmation" },
                { "@dataSource", "Today Report" },
                { "@format", "TradeConfirm" },
                { "@frequency", "Intraday" },
                { "@contractUnit", DBNull.Value },
                { "@contractUnitType", DBNull.Value },
                { "@priceQuotation", DBNull.Value },
                { "@minimumPriceFluctuation", DBNull.Value },
                { "@currency", (object)currency ?? DBNull.Value },
                { "@listingExchange", DBNull.Value },
                { "@conId", conid }
            };

            ExecuteCommand(connection, transaction, insertQuery, parameters);
        }

        #endregion
    }
}
