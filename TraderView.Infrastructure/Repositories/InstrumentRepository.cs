using Microsoft.Data.SqlClient;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Engineering;
using TraderView.Application.Features.Instruments.Command.Create;
using TraderView.Application.Features.Instruments.Query.GetBy;
using TraderView.Application.Features;
using Microsoft.EntityFrameworkCore;
using TraderView.Application.Features.TradeExecutions.Query.GetBy;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Application.Mappers;
using TraderView.Domain.Entities;
using TraderView.Infrastructure.DbContexts;

namespace TraderView.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for Instrument-related database operations
    /// </summary>
    public class InstrumentRepository : EfBaseRepository<Instrument>, IInstrumentRepository
    {
        private readonly SqlRepositoryAdapter _sqlAdapter;

        public InstrumentRepository(AppDbContext db, IDbConnectionFactory connectionFactory) : base(db)
        {
            _sqlAdapter = new SqlRepositoryAdapter(connectionFactory);
        }

        // Adapter to reuse the SQL helper methods from BaseRepository via composition.
        private sealed class SqlRepositoryAdapter : BaseRepository
        {
            public SqlRepositoryAdapter(IDbConnectionFactory connectionFactory) : base(connectionFactory)
            {
            }

            public new void ExecuteDatabaseOperation(Action<SqlConnection> operation) => base.ExecuteDatabaseOperation(operation);
            public new T ExecuteDatabaseOperation<T>(Func<SqlConnection, T> operation) => base.ExecuteDatabaseOperation(operation);
            public new void ExecuteCommand(SqlConnection connection, SqlTransaction transaction, IQueryWithParameters queryWithParameters) => base.ExecuteCommand(connection, transaction, queryWithParameters);
            public new T ExecuteScalar<T>(SqlConnection connection, SqlTransaction transaction, IQueryWithParameters queryWithParams) => base.ExecuteScalar<T>(connection, transaction, queryWithParams);
            public new T? ExecuteSingle<T>(SqlConnection connection, SqlTransaction? transaction, Func<SqlDataReader, T> mapFunction, IQueryWithParameters queryWithParameters) where T : class => base.ExecuteSingle(connection, transaction, mapFunction, queryWithParameters);
            public new List<T> ExecuteList<T>(SqlConnection connection, SqlTransaction? transaction, Func<SqlDataReader, T> mapFunction, IQueryWithParameters queryWithParameters) => base.ExecuteList(connection, transaction, mapFunction, queryWithParameters);
        }
        void IInstrumentRepository.UpsertInstruments(List<TradeConfirm> tradeConfirms, string source)
        {
           UpsertInstrumentsAsync(ConvertToTradeExecute(tradeConfirms), source).GetAwaiter().GetResult();
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

        public async Task UpsertInstrumentsAsync(List<TradeConfirm> tradeConfirms, string source)
        {
            await UpsertInstrumentsAsync(ConvertToTradeExecute(tradeConfirms), source).ConfigureAwait(false);
        }

        public async Task UpsertInstrumentsAsync(List<TradeExecution> trades, string source)
        {
            if (trades == null || !trades.Any())
                return;

            var uniqueConids = trades
                .Where(t => !string.IsNullOrEmpty(t.Conid))
                .Select(t => t.Conid)
                .Distinct()
                .ToList();

            int createdCount = 0;
            int existingCount = 0;

            foreach (var conid in uniqueConids)
            {
                int? instrumentId = await GetInstrumentIdByConIdAsync(conid).ConfigureAwait(false);

                if (!instrumentId.HasValue)
                {
                    var trade = trades.First(t => t.Conid == conid);

                    await InsertInstrumentAsync(
                        conid,
                        trade.Symbol,
                        trade.ListingExchange,
                        trade.Currency,
                        trade.AssetCategory,
                        source,
                        trade.Symbol).ConfigureAwait(false);

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

            foreach (var trade in trades.Where(x => x.Position.InstrumentId == 0))
            {
                if (!string.IsNullOrEmpty(trade.Conid))
                {
                    int? instrumentId = await GetInstrumentIdByConIdAsync(trade.Conid).ConfigureAwait(false);
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
                var instrument = _sqlAdapter.ExecuteDatabaseOperation(connection =>
                {
                    return _sqlAdapter.ExecuteSingle(connection, null, MapFromReader.MapInstrument, new GetInstrumentByIdQuery(instrumentId));
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
            return GetInstrumentIdByConIdAsync(conid).GetAwaiter().GetResult();
        }

        public async Task<int?> GetInstrumentIdByConIdAsync(string conid)
        {
            if (string.IsNullOrEmpty(conid))
                return null;

            var id = await _db.Set<Instrument>()
                .Where(i => i.ConId == conid)
                .Select(i => i.Id)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            return id > 0 ? id : (int?)null;
        }
        public int? GetInstrumentIdFromSymbol(string symbol, string provider)
        {
            return GetInstrumentIdFromSymbolAsync(symbol, provider).GetAwaiter().GetResult();
        }

        public async Task<int?> GetInstrumentIdFromSymbolAsync(string symbol, string provider)
        {
            if (string.IsNullOrEmpty(symbol) || string.IsNullOrEmpty(provider))
                return null;

            var id = await _db.Set<Instrument>()
                .Where(i => i.InstrumentName == symbol && i.Provider == provider)
                .Select(i => i.Id)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            return id > 0 ? id : (int?)null;
        }

        private int? GetInstrumentIdBySymbol(SqlConnection connection, SqlTransaction transaction, string symbol, string provider)
        {
            int instrumentId = _sqlAdapter.ExecuteScalar<int>(connection, transaction, new GetInstrumentBySymbolAndProviderQuery(symbol, provider));
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
            return InsertInstrumentAsync(conid, symbol, listingExchange, currency, assetCategory, provider, dataSource).GetAwaiter().GetResult();
        }

        public async Task<int> InsertInstrumentAsync(
            string conid,
            string symbol,
            string listingExchange,
            string currency,
            string assetCategory,
            string provider,
            string dataSource)
        {
            var instrument = new Instrument
            {
                InstrumentName = symbol ?? string.Empty,
                Provider = provider,
                DataName = symbol,
                DataSource = dataSource,
                Format = "TradeExecution",
                Frequency = "D",
                ContractUnit = null,
                ContractUnitType = assetCategory,
                PriceQuotation = null,
                MinimumPriceFluctuation = null,
                Currency = currency,
                ListingExchange = listingExchange,
                ConId = conid
            };

            var added = await AddAsync(instrument).ConfigureAwait(false);
            return added.Id;
        }

        /// <summary>
        /// Gets an instrument by its ID asynchronously
        /// </summary>
        public async Task<Instrument?> GetByIdAsync(int instrumentId)
        {
            return await base.GetByIdAsync(instrumentId).ConfigureAwait(false);
        }

        // Async wrapper matching IInstrumentRepository
        public Task<Instrument?> GetAsync(int instrumentId)
        {
            return GetByIdAsync(instrumentId);
        }

        
        #endregion
    }
}
