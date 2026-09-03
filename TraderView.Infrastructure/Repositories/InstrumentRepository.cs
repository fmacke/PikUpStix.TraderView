using Microsoft.Data.SqlClient;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Engineering;
using TraderView.Application.Features.Instruments.Command.Create;
using TraderView.Application.Features.Instruments.Query.GetBy;
using TraderView.Application.Features.TradeExecutions.Query.GetBy;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Mappers;
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
                    Position = new Position { Id = 0, InstrumentId = 0},
                    PositionId =0
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
            foreach (var trade in trades.Where(x => x.Position.InstrumentId == 0))
            {
                if (!string.IsNullOrEmpty(trade.Conid))
                {
                    int? instrumentId = GetInstrumentIdByConId(trade.Conid);
                    if (instrumentId.HasValue)
                    {
                        trade.Position.InstrumentId = instrumentId.Value;
                    }
                }
            }
        }



        #region Private Helper Methods
        public Instrument Get(int instrumentId)
        {
            try
            {
                var instrument = ExecuteDatabaseOperation(connection =>
                {                    
                    return ExecuteSingle(connection, null, MapFromReader.MapInstrument, new GetInstrumentByIdQuery(instrumentId));
                });
                if (instrument == null)
                    throw new InvalidOperationException($"Instrument with Id {instrumentId} was not found.");                
                return instrument;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving instrument by ID {instrumentId}: {ex.Message}");
                throw;
            }
        }

        public int? GetInstrumentIdByConId(string conid)
        {
            int instrumentId = 0;
            ExecuteDatabaseOperation(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    instrumentId = ExecuteScalar<int>(connection, transaction, new GetInstrumentByConIdQuery(conid));
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
            int instrumentId = ExecuteScalar<int>(connection, transaction, new GetInstrumentBySymbolAndProviderQuery(symbol, provider));
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
                    newInstrumentId = ExecuteScalar<int>(connection, transaction, new CreateInstrumentCommand(symbol, provider, symbol, dataSource, "TradeExecution", "D", null, assetCategory, null, null, currency, listingExchange, int.Parse(conid)));
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
