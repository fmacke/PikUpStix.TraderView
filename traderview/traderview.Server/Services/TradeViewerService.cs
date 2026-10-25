using Microsoft.Data.SqlClient;
using System.Data;
using traderview.Server.DTOs;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;

namespace traderview.Server.Services
{
    // TODO: Consider refactoring this service to separate concerns, e.g., data access, business logic, and mapping to DTOs.
    // REALLY consider using AutoMapper or similar for mapping domain models to DTOs to reduce boilerplate code.
    // ALSO, SERVICES SHOULD BE IN CORE PROJECT, NOT IN SERVER PROJECT. Move this to a core project and inject dependencies properly.
    public class TradeViewerService : ITradeViewerService
    {
        
        private readonly ITradeExecutionRepository _tradeExecutionRepository;
        private readonly ITradeHistoryReportService _tradeHistoryReportService;
        private readonly IInstrumentRepository _instrumentRepository;
        private readonly IHistoricalDataRepository _historicalDataRepository;

        public TradeViewerService(
            ILogger<TradeViewerService> logger,
            ITradeExecutionRepository tradeExecutionRepository,
            ITradeHistoryReportService tradeHistoryReportService,
            IInstrumentRepository instrumentRepository,
            IHistoricalDataRepository historicalDataRepository)
        {
            
            _tradeExecutionRepository = tradeExecutionRepository;
            _tradeHistoryReportService = tradeHistoryReportService;
            _instrumentRepository = instrumentRepository;
            _historicalDataRepository = historicalDataRepository;
        }

        public async Task<List<TradeDto>> GetAllTradesAsync()
        {
            try
            {
                // Use repository to get trade executions and create history
                var tradeExecutions = await _tradeExecutionRepository.GetTradeExecutionsAsync();
                _tradeHistoryReportService.CreateTradeHistoryReport(tradeExecutions);
                var trades = _tradeHistoryReportService.TradeHistoryAggregated;

                // Map HistoricalTrade to TradeDto
                return await Task.FromResult(trades.Select(trade => new TradeDto
                {
                    PositionId = Convert.ToInt32(trade.PositionId),
                    InstrumentId = trade.InstrumentId,
                    Symbol = trade.Symbol,
                    EntryDate = trade.TradeOpened,
                    ExitDate = trade.TradeClosed,
                    EntryPrice = Convert.ToDecimal(trade.TradePrice),
                    ExitPrice = Convert.ToDecimal(trade.ClosePrice),
                    Quantity = Math.Abs(Convert.ToDecimal(trade.Quantity)),
                    Pnl = Convert.ToDecimal(trade.RealizedPnL),
                    BuySell = trade.Quantity > 0 ? "BUY" : "SELL"
                }).ToList());
            }
            catch (Exception ex)
            {
                // Logging removed
                throw;
            }
        }

        public async Task<TradeDetailDto?> GetTradeDetailAsync(int positionId)
        {
            try
            {
                // Get trade summary using repository (positionId is the close order ID)
                var tradeSummary = await _tradeExecutionRepository.GetTradeSummaryByPositionIdAsync(positionId);
                if (tradeSummary == null) return null;

                var trade = MapToTradeDto(tradeSummary);

                // Get instrument details using repository
                var instrumentDomain = await _instrumentRepository.GetByIdAsync(trade.InstrumentId);
                if (instrumentDomain == null) return null;

                var instrument = new InstrumentDto
                {
                    Id = instrumentDomain.Id,
                    InstrumentName = instrumentDomain.InstrumentName ?? string.Empty,
                    Provider = instrumentDomain.Provider ?? string.Empty,
                    DataName = instrumentDomain.DataName ?? string.Empty,
                    Currency = instrumentDomain.Currency ?? string.Empty,
                    ListingExchange = instrumentDomain.ListingExchange
                };


                var executions = (await _tradeExecutionRepository.GetTradeExecutionsByPositionIdAsync(positionId)).Select(e => new TradeExecutionDto
                {
                    Id = e.Id,
                    PositionId = positionId,
                    InstrumentId = e.Position.InstrumentId,
                    Symbol = e.Symbol,
                    TradeId = e.TradeId,
                    DateTime = e.DateTime.ToString("yyyyMMdd"),
                    TradeDate = e.TradeDate,  
                    Quantity = e.Quantity,  
                    TradePrice = e.TradePrice,
                    BuySell = e.BuySell,
                    FifoPnlRealized = e.FifoPnlRealized,
                    IbCommission = e.IbCommission
                }).ToList();

                return new TradeDetailDto
                {
                    Trade = trade,
                    Instrument = instrument,
                    Executions = executions
                };
            }
            catch (Exception ex)
            {
                // Logging removed
                throw;
            }
        }

        public async Task<TradeContextDto?> GetTradeContextAsync(int positionId, int daysBefore = 150, int daysAfter = 150)
        {
            try
            {
                // Get trade summary using repository (positionId is the close order ID)
                var tradeSummary = await _tradeExecutionRepository.GetTradeSummaryByPositionIdAsync(positionId);
                if (tradeSummary == null) return null;

                var trade = MapToTradeDto(tradeSummary);

                // Get executions sorted by date (FIFO basis) for visualization
                var executionsDomain = await _tradeExecutionRepository.GetTradeExecutionsByPositionIdAsync(positionId);
                trade.Executions = executionsDomain
                    .OrderBy(e => e.DateTime)
                    .Select(e => new TradeExecutionDto
                    {
                        Id = e.Id,
                        PositionId = positionId,
                        InstrumentId = trade.InstrumentId,
                        Symbol = e.Symbol,
                        TradeId = e.TradeId,
                        DateTime = e.DateTime.ToString("yyyy-MM-dd"),
                        Date = e.DateTime.ToString("yyyy-MM-dd"),
                        TradeDate = e.TradeDate,
                        Quantity = e.Quantity,
                        TradePrice = e.TradePrice,
                        Price = e.TradePrice,
                        BuySell = e.BuySell,
                        Side = e.BuySell,
                        FifoPnlRealized = e.FifoPnlRealized,
                        IbCommission = e.IbCommission,
                        Commission = e.IbCommission,
                        IbCommissionCurrency = e.IbCommissionCurrency,
                        ClosePrice = e.ClosePrice,
                        Cost = e.Cost,
                        IbExecId = e.IbExecId
                    }).ToList();

                // Get candlestick data around the trade dates using repository
                var candlesticksDomain = await _historicalDataRepository.GetCandlesticksAsync(
                    trade.InstrumentId,
                    trade.EntryDate.AddDays(-daysBefore),
                    trade.ExitDate.AddDays(daysAfter)
                );

                var candlesticks = candlesticksDomain.Select(c => new CandlestickDto
                {
                    Date = c.Date,
                    Open = c.OpenPrice,
                    High = c.HighPrice,
                    Low = c.LowPrice,
                    Close = c.ClosePrice,
                    Volume = c.Volume,
                    InstrumentId = trade.InstrumentId
                }).ToList();

                return new TradeContextDto
                {
                    Trade = trade,
                    Candlesticks = candlesticks,
                    EntryDate = trade.EntryDate,
                    ExitDate = trade.ExitDate
                };
            }
            catch (Exception ex)
            {
                // Logging removed
                throw;
            }
        }

        private TradeDto MapToTradeDto(TraderView.Domain.Entities.TradeSummary tradeSummary)
        {
            return new TradeDto
            {
                PositionId = tradeSummary.PositionId,
                InstrumentId = tradeSummary.InstrumentId,
                Symbol = tradeSummary.Symbol,
                EntryDate = tradeSummary.EntryDate,
                ExitDate = tradeSummary.ExitDate,
                EntryPrice = tradeSummary.EntryPrice,
                ExitPrice = tradeSummary.ExitPrice,
                Quantity = tradeSummary.Quantity,
                Pnl = tradeSummary.Pnl,
                BuySell = tradeSummary.BuySell
            };
        }

        private async Task<InstrumentDto?> GetInstrumentAsync(SqlConnection connection, int instrumentId)
        {
            var query = @"
                SELECT Id, InstrumentName, Provider, DataName, Currency, ListingExchange
                FROM Instruments
                WHERE Id = @InstrumentId";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@InstrumentId", instrumentId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new InstrumentDto
                {
                    Id = reader.GetInt32("Id"),
                    InstrumentName = reader.GetString("InstrumentName"),
                    Provider = reader.GetString("Provider"),
                    DataName = reader.GetString("DataName"),
                    Currency = reader.GetString("Currency"),
                    ListingExchange = reader.IsDBNull("ListingExchange") ? null : reader.GetString("ListingExchange")
                };
            }

            return null;
        }
    }
}

