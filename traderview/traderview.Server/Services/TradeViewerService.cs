using Microsoft.Data.SqlClient;
using System.Data;
using traderview.Server.DTOs;
using IKBR_Report_Puller.Interfaces;

namespace traderview.Server.Services
{
    public class TradeViewerService : ITradeViewerService
    {
        private readonly string _connectionString;
        private readonly ILogger<TradeViewerService> _logger;
        private readonly IDataService _dataService;
        private readonly ITradeHistoryReportService _tradeHistoryReportService;

        public TradeViewerService(
            IConfiguration configuration, 
            ILogger<TradeViewerService> logger,
            IDataService dataService,
            ITradeHistoryReportService tradeHistoryReportService)
        {
            _connectionString = configuration.GetConnectionString("TradingDatabase") 
                ?? throw new InvalidOperationException("Connection string 'TradingDatabase' not found.");
            _logger = logger;
            _dataService = dataService;
            _tradeHistoryReportService = tradeHistoryReportService;
        }

        public async Task<List<TradeDto>> GetAllTradesAsync()
        {
            try
            {
                // Use existing service to get trade executions and create history
                var tradeExecutions = _dataService.GetTradeExecutions();
                _tradeHistoryReportService.CreateTradeHistoryReport(tradeExecutions);
                var trades = _tradeHistoryReportService.TradeHistoryAggregated;

                // Map HistoricalTrade to TradeDto
                return await Task.FromResult(trades.Select(trade => new TradeDto
                {
                    Id = trade.OpenIbOrderID,
                    InstrumentId = trade.InstrumentId,
                    Symbol = trade.Symbol,
                    EntryDate = trade.TradeOpened,
                    ExitDate = trade.TradeClosed,
                    EntryPrice = trade.AveragePrice,
                    ExitPrice = trade.ClosePrice,
                    Quantity = Math.Abs(trade.Quantity),
                    Pnl = trade.RealizedPnL,
                    BuySell = trade.IsLong ? "BUY" : "SELL"
                }).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trades");
                throw;
            }
        }

        public async Task<TradeDetailDto?> GetTradeDetailAsync(long tradeId)
        {
            try
            {
                // Get trade summary using DataService
                var tradeSummary = await Task.Run(() => _dataService.GetTradeSummaryByOrderId(tradeId));
                if (tradeSummary == null) return null;

                var trade = MapToTradeDto(tradeSummary);

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Get instrument details
                var instrument = await GetInstrumentAsync(connection, trade.InstrumentId);
                if (instrument == null) return null;

                // Get trade executions
                var executions = await GetTradeExecutionsAsync(connection, tradeId);

                return new TradeDetailDto
                {
                    Trade = trade,
                    Instrument = instrument,
                    Executions = executions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trade detail for trade {TradeId}", tradeId);
                throw;
            }
        }

        public async Task<TradeContextDto?> GetTradeContextAsync(long tradeId, int daysBefore = 30, int daysAfter = 30)
        {
            try
            {
                // Get trade summary using DataService
                var tradeSummary = await Task.Run(() => _dataService.GetTradeSummaryByOrderId(tradeId));
                if (tradeSummary == null) return null;

                var trade = MapToTradeDto(tradeSummary);

                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Get candlestick data around the trade dates
                var candlesticks = await GetCandlesticksAsync(
                    connection, 
                    trade.InstrumentId, 
                    trade.EntryDate.AddDays(-daysBefore), 
                    trade.ExitDate.AddDays(daysAfter)
                );

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
                _logger.LogError(ex, "Error fetching trade context for trade {TradeId}", tradeId);
                throw;
            }
        }

        private TradeDto MapToTradeDto(IKBR_Report_Puller.Domain.TradeSummary tradeSummary)
        {
            return new TradeDto
            {
                Id = tradeSummary.Id,
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

        private async Task<List<TradeExecutionDto>> GetTradeExecutionsAsync(SqlConnection connection, long tradeId)
        {
            var executions = new List<TradeExecutionDto>();

            var query = @"
                SELECT 
                    id, InstrumentId, symbol, tradeID, dateTime, tradeDate, 
                    quantity, tradePrice, buySell, fifoPnlRealized, ibCommission
                FROM TradeExecutions
                WHERE ibOrderID = @TradeId
                ORDER BY tradeDate, dateTime";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@TradeId", tradeId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                executions.Add(new TradeExecutionDto
                {
                    Id = reader.GetInt32("id"),
                    InstrumentId = reader.GetInt32("InstrumentId"),
                    Symbol = reader.IsDBNull("symbol") ? null : reader.GetString("symbol"),
                    TradeID = reader.IsDBNull("tradeID") ? null : reader.GetInt64("tradeID"),
                    DateTime = reader.IsDBNull("dateTime") ? null : reader.GetString("dateTime"),
                    TradeDate = reader.IsDBNull("tradeDate") ? null : reader.GetDateTime("tradeDate"),
                    Quantity = reader.IsDBNull("quantity") ? null : reader.GetDecimal("quantity"),
                    TradePrice = reader.IsDBNull("tradePrice") ? null : reader.GetDecimal("tradePrice"),
                    BuySell = reader.IsDBNull("buySell") ? null : reader.GetString("buySell"),
                    FifoPnlRealized = reader.IsDBNull("fifoPnlRealized") ? null : reader.GetDecimal("fifoPnlRealized"),
                    IbCommission = reader.IsDBNull("ibCommission") ? null : reader.GetDecimal("ibCommission")
                });
            }

            return executions;
        }

        private async Task<List<CandlestickDto>> GetCandlesticksAsync(
            SqlConnection connection, 
            int instrumentId, 
            DateTime startDate, 
            DateTime endDate)
        {
            var candlesticks = new List<CandlestickDto>();

            var query = @"
                SELECT Date, OpenPrice, HighPrice, LowPrice, ClosePrice, Volume
                FROM HistoricalData
                WHERE InstrumentId = @InstrumentId
                    AND Date >= @StartDate
                    AND Date <= @EndDate
                ORDER BY Date ASC";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@InstrumentId", instrumentId);
            command.Parameters.AddWithValue("@StartDate", startDate);
            command.Parameters.AddWithValue("@EndDate", endDate);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                candlesticks.Add(new CandlestickDto
                {
                    Date = reader.GetDateTime("Date"),
                    Open = reader.GetDouble("OpenPrice"),
                    High = reader.GetDouble("HighPrice"),
                    Low = reader.GetDouble("LowPrice"),
                    Close = reader.GetDouble("ClosePrice"),
                    Volume = reader.GetDouble("Volume")
                });
            }

            return candlesticks;
        }
    }
}
