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
        public InstrumentRepository(AppDbContext db, IDbConnectionFactory connectionFactory) : base(db)
        {
        }        
        public async Task UpsertInstrumentsAsync(List<TradeConfirm> tradeConfirms, string source)
        {
            await UpsertInstrumentsAsync(ConvertToTradeExecute(tradeConfirms), source);
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
                int? instrumentId = await GetInstrumentIdByConIdAsync(conid);

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
                    int? instrumentId = await this.InsertInstrumentAsync  GetInstrumentIdByConIdAsync(trade.Conid).ConfigureAwait(false);
                    if (instrumentId.HasValue)
                    {
                        trade.Position.InstrumentId = instrumentId.Value;
                    }
                }
            }
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
                    Position = new Position { Id = 0, InstrumentId = 0 },
                    PositionId = 0
                };
                tradeExecutions.Add(execution);
            }
            return tradeExecutions;
        }
        private async Task<int> InsertInstrumentAsync(string conid,string symbol,string listingExchange,string currency,string assetCategory,string provider,string dataSource)
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

        Task<int?> IInstrumentRepository.GetInstrumentIdByConIdAsync(string conid)
        {
            //SORT THIS PLACEHOLDER METHOD OUT LATER, FOR NOW JUST RETURN 1
            return Task.FromResult<int?>(1);
        }
    }
}
