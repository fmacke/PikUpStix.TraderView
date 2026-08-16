using Microsoft.Data.SqlClient;
using TraderView.Application.Features.Instruments.Command.Create;
using TraderView.Application.Features.Instruments.Query.GetBy;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Domain.Entities;

namespace TraderView.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for Instrument-related database operations
    /// </summary>
    public class InstrumentRepository : BaseRepository, IInstrumentRepository
    {
        public InstrumentRepository(string connectionString) : base(connectionString)
        {
        }
        void IInstrumentRepository.UpsertInstruments(List<TradeConfirm> tradeConfirms, string source)
        {
           UpsertInstruments(ConvertToTradeExecute(tradeConfirms), source);
        }
        private List<TradeExecution> ConvertToTradeExecute(List<TradeConfirm> tradeConfirms)
        {
            // Convert TradeConfirm objects to TradeExecution objects for instrument upsertion
            var tradeExecutions = new List<TradeExecution>();
            foreach (var confirm in tradeConfirms)
            {
                var execution = new TradeExecution
                {
                    Conid = confirm.Conid,
                    Symbol = confirm.Symbol,
                    Description = confirm.Description,
                    AssetCategory = confirm.AssetCategory,
                    Currency = confirm.Currency,
                    TradeDate = confirm.TradeDate,
                    TradePrice = confirm.TradePrice,
                    Quantity = confirm.Quantity,
                    TransactionType = confirm.TransactionType,
                    Exchange = confirm.Exchange,
                    ListingExchange = confirm.ListingExchange,
                };
                tradeExecutions.Add(execution);
            }
            return tradeExecutions;
        }
        /// <summary>
        /// Ensures instruments exist for the given trades
        /// Creates missing instruments automatically
        /// </summary>
        public void UpsertInstruments(List<TradeExecution> trades, string source)
        {
            if (trades == null || !trades.Any())
                return;
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
                    int? instrumentId = GetInstrumentIdByConId(conid);

                    if (!instrumentId.HasValue)
                    {
                        var trade = trades.First(t => t.Conid == conid);

                        ((IInstrumentRepository)this).InsertInstrument(
                            conid,
                            trade.Symbol,
                            trade.ListingExchange,
                            trade.Currency,
                            trade.AssetCategory,
                            source,
                            trade.Symbol);

                        createdCount++;
                    }
                    else
                    {
                        existingCount++;
                    }
                }
                if (createdCount > 0)
                {
                    Console.WriteLine($"Created {createdCount} new instrument(s), {existingCount} already existed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error upserting instruments: {ex.Message}");
                throw;
            }
            foreach (var trade in trades.Where(x => x.InstrumentId == 0))
            {
                if (!string.IsNullOrEmpty(trade.Conid))
                {
                    int? instrumentId = GetInstrumentIdByConId(trade.Conid);
                    if (instrumentId.HasValue)
                    {
                        trade.InstrumentId = instrumentId.Value;
                    }
                }
            }
        }

        

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

        public int? GetInstrumentIdByConId(string conid)
        {
            int instrumentId = 0;
            ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    string query = new GetInstrumentByConIdQuery().Script();
                    var parameters = new Dictionary<string, object>{{ "@conid", conid }};
                    instrumentId = ExecuteScalar<int>(connection, transaction, query, parameters);
                    transaction.Commit();
                }
            });
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
            transaction.Commit();
            return instrumentId > 0 ? instrumentId : (int?)null;
        }

        int IInstrumentRepository.InsertInstrument(
            string conid,
            string symbol,
            string listingExchange,
            string currency,
            string assetCategory,
            string provider,
            string dataSource)
        {
            int newInstrumentId = 0;
            ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    string insertQuery = new CreateInstrumentCommand().Script();

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
                    newInstrumentId = ExecuteScalar<int>(connection, transaction, insertQuery, parameters);
                    transaction.Commit();
                }
            });
            return newInstrumentId;
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
                        SELECT Id, InstrumentName, Provider, DataName, Currency, ListingExchange, DataSource
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
                                ? string.Empty : reader.GetString(reader.GetOrdinal("ListingExchange")),
                            DataSource = reader.IsDBNull(reader.GetOrdinal("DataSource"))
                                ? string.Empty : reader.GetString(reader.GetOrdinal("DataSource")),
                        };
                    }
                });
                return instrument;
            });
        }

        
        #endregion
    }
}
