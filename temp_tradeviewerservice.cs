using Microsoft.Data.SqlClient;
using System.Data;
using traderview.Server.DTOs;
using PikUpStix.TraderView.Interfaces;

namespace traderview.Server.Services
{
    public class TradeViewerService : ITradeViewerService
    {
        private readonly ILogger<TradeViewerService> _logger;
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
            _logger = logger;
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
                var tradeExecutions = _tradeExecutionRepository.GetTradeExecutions();
                _tradeHistoryReportService.CreateTradeHistoryReport(tradeExecutions);
                var trades = _tradeHistoryReportService.TradeHistoryAggregated;

                // Map HistoricalTrade to TradeDto
                return await Task.FromResult(trades.Select(trade => new TradeDto
                {
                    PositionId = trade.PositionId,
                    InstrumentId = trade.InstrumentId,
                    Symbol = trade.Symbol,
                    EntryDate = trade.TradeOpened,
                    ExitDate = trade.TradeClosed,
                    EntryPrice = trade.TradePrice,
                    ExitPrice = trade.ClosePrice,
                    Quantity = Math.Abs(trade.Quantity),
                    Pnl = trade.RealizedPnL,
                    BuySell = trade.Quantity > 0 ? "BUY" : "SELL"
                }).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trades");
                throw;
            }
        }

        public async Task<TradeDetailDto?> GetTradeDetailAsync(int positionId)
        {
            try
            {
                // Get trade summary using repository (positionId is the close order ID)
                var tradeSummary = await Task.Run(() => _tradeExecutionRepository.GetTradeSummaryByPositionId(positionId));
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

                // Get trade executions using repository
                var executionsDomain = await _tradeExecutionRepository.GetByPositionIdAsync(positionId);
                var executions = executionsDomain.Select(e => new TradeExecutionDto
                {
                    Id = e.Id,
                    PositionId = positionId,
                    InstrumentId = e.InstrumentId,
                    Symbol = e.Symbol,
                    TradeID = e.TradeID,
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
                _logger.LogError(ex, "Error fetching trade detail for position {PositionId}", positionId);
                throw;
            }
        }

        public async Task<TradeContextDto?> GetTradeContextAsync(int positionId, int daysBefore = 150, int daysAfter = 150)
        {
            try
            {
                // Get trade summary using repository (positionId is the close order ID)
                var tradeSummary = await Task.Run(() => _tradeExecutionRepository.GetTradeSummaryByPositionId(positionId));
                if (tradeSummary == null) return null;

                var trade = MapToTradeDto(tradeSummary);

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
                _logger.LogError(ex, "Error fetching trade context for position {PositionId}", positionId);
                throw;
            }
        }

        private TradeDto MapToTradeDto(IKBR_Report_Puller.Domain.TradeSummary tradeSummary)
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

        private async Task<List<TradeExecutionDto>> GetTradeExecutionsAsync(SqlConnection connection, int positionId)
        {
            var executions = new List<TradeExecutionDto>();

            var query = @"
                SELECT 
                    id, InstrumentId, symbol, tradeID, dateTime, tradeDate, 
                    quantity, tradePrice, buySell, fifoPnlRealized, ibCommission
                FROM TradeExecutions
                WHERE PositionID = @PositionId
                ORDER BY tradeDate, dateTime";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PositionId", positionId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                executions.Add(new TradeExecutionDto
                {
                    Id = reader.GetInt32("id"),
                    InstrumentId = reader.GetInt32("InstrumentId"),
                    PositionId = positionId,
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
                    Volume = reader.GetDouble("Volume"),
                    InstrumentId = instrumentId
                });
            }

            return candlesticks;
        }

        public async Task<RSIndicatorDataDto?> GetRSIndicatorDataAsync(int positionId, string benchmarkSymbol = "^GSPC", int daysBefore = 150, int daysAfter = 150)
        {
            try
            {
                // Get trade summary (positionId )
                var tradeSummary = await Task.Run(() => _tradeExecutionRepository.GetTradeSummaryByPositionId(positionId));
                if (tradeSummary == null) return null;

                var trade = MapToTradeDto(tradeSummary);

                // Get stock candlestick data using repository
                var stockCandlesticksDomain = await _historicalDataRepository.GetCandlesticksAsync(
                    trade.InstrumentId,
                    trade.EntryDate.AddDays(-daysBefore),
                    trade.ExitDate.AddDays(daysAfter)
                );

                var stockCandlesticks = stockCandlesticksDomain.Select(c => new CandlestickDto
                {
                    Date = c.Date,
                    Open = c.OpenPrice,
                    High = c.HighPrice,
                    Low = c.LowPrice,
                    Close = c.ClosePrice,
                    Volume = c.Volume,
                    InstrumentId = trade.InstrumentId
                }).ToList();

                if (stockCandlesticks.Count == 0)
                {
                    _logger.LogWarning("No stock price data available for trade {TradeId}", positionId);
                    return null;
                }

                // Get benchmark candlestick data (SPX or similar) using repository
                var benchmarkInstrumentId = await _historicalDataRepository.GetInstrumentIdBySymbolAsync(benchmarkSymbol);
                if (benchmarkInstrumentId == null)
                {
                    _logger.LogWarning("Benchmark instrument {BenchmarkSymbol} not found in database", benchmarkSymbol);
                    return null;
                }

                var benchmarkCandlesticksDomain = await _historicalDataRepository.GetCandlesticksAsync(
                    benchmarkInstrumentId.Value,
                    trade.EntryDate.AddDays(-daysBefore),
                    trade.ExitDate.AddDays(daysAfter)
                );

                var benchmarkCandlesticks = benchmarkCandlesticksDomain.Select(c => new CandlestickDto
                {
                    Date = c.Date,
                    Open = c.OpenPrice,
                    High = c.HighPrice,
                    Low = c.LowPrice,
                    Close = c.ClosePrice,
                    Volume = c.Volume,
                    InstrumentId = benchmarkInstrumentId.Value
                }).ToList();

                if (benchmarkCandlesticks.Count == 0)
                {
                    _logger.LogWarning("No benchmark data available for {BenchmarkSymbol}", benchmarkSymbol);
                    // Return null or stub data - for now return null
                    return null;
                }

                // Calculate RS data
                var rsDataPoints = CalculateRSData(stockCandlesticks, benchmarkCandlesticks);

                // Calculate metrics
                var metrics = CalculateRSMetrics(stockCandlesticks, rsDataPoints, trade);

                return new RSIndicatorDataDto
                {
                    RSData = rsDataPoints,
                    Metrics = metrics
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating RS indicator for trade {TradeId}", positionId);
                throw;
            }
        }

        private async Task<List<CandlestickDto>> GetBenchmarkCandlesticksAsync(
            SqlConnection connection,
            string benchmarkSymbol,
            DateTime startDate,
            DateTime endDate)
        {
            // Try to find benchmark instrument by symbol (e.g., SPX, ^GSPC, etc.)
            var instrumentQuery = @"
                SELECT Id 
                FROM Instruments 
                WHERE InstrumentName = @Symbol";

            int? benchmarkInstrumentId = null;

            using (var command = new SqlCommand(instrumentQuery, connection))
            {
                command.Parameters.AddWithValue("@Symbol", benchmarkSymbol);
                var result = await command.ExecuteScalarAsync();
                if (result != null)
                {
                    benchmarkInstrumentId = Convert.ToInt32(result);
                }
            }

            if (benchmarkInstrumentId == null)
            {
                _logger.LogWarning("Benchmark instrument {BenchmarkSymbol} not found in database", benchmarkSymbol);
                return new List<CandlestickDto>();
            }

            return await GetCandlesticksAsync(connection, benchmarkInstrumentId.Value, startDate, endDate);
        }

        private List<RSDataPointDto> CalculateRSData(List<CandlestickDto> stockData, List<CandlestickDto> benchmarkData)
        {
            var rsDataPoints = new List<RSDataPointDto>();

            // Create a dictionary of benchmark closes by date for fast lookup
            var benchmarkByDate = benchmarkData.ToDictionary(c => c.Date.Date, c => c.Close);

            // Calculate RS ratio for each date where both stock and benchmark data exist
            var rsRatios = new List<(DateTime Date, double RSRatio)>();

            foreach (var stockCandle in stockData)
            {
                if (benchmarkByDate.TryGetValue(stockCandle.Date.Date, out var benchmarkClose) && benchmarkClose > 0)
                {
                    double rsRatio = stockCandle.Close / benchmarkClose;
                    rsRatios.Add((stockCandle.Date, rsRatio));
                }
            }

            if (rsRatios.Count == 0) return rsDataPoints;

            // Calculate 21-period moving average of RS ratio
            const int maLength = 21;
            var rsMAValues = CalculateSMA(rsRatios.Select(r => r.RSRatio).ToList(), maLength);

            // Calculate 50-bar and 52-week (252 bars) highs
            const int rs50Period = 50;
            const int rs252Period = 252;

            for (int i = 0; i < rsRatios.Count; i++)
            {
                var (date, rsRatio) = rsRatios[i];
                double rsMA = i >= maLength - 1 ? rsMAValues[i] : rsRatio;

                // Check if RS is at 50-bar high
                int lookback50 = Math.Min(rs50Period, i + 1);
                double rs50High = rsRatios.Skip(Math.Max(0, i - lookback50 + 1)).Take(lookback50).Max(r => r.RSRatio);
                bool isRS50High = Math.Abs(rsRatio - rs50High) < 0.0001;

                // Check if RS is at 52-week high (252 bars)
                int lookback252 = Math.Min(rs252Period, i + 1);
                double rs252High = rsRatios.Skip(Math.Max(0, i - lookback252 + 1)).Take(lookback252).Max(r => r.RSRatio);
                bool isRSNewHigh = Math.Abs(rsRatio - rs252High) < 0.0001;

                // Check if price is at 52-week high (for blue dot logic)
                var stockCandle = stockData[i];
                int priceLookback252 = Math.Min(rs252Period, i + 1);
                double price52High = stockData.Skip(Math.Max(0, i - priceLookback252 + 1)).Take(priceLookback252).Max(c => c.High);
                bool isBlueDot = isRSNewHigh && stockCandle.Close < price52High;

                rsDataPoints.Add(new RSDataPointDto
                {
                    Date = date,
                    RSRatio = rsRatio,
                    RSMA = rsMA,
                    IsRS50High = isRS50High,
                    IsRSNewHigh = isRSNewHigh,
                    IsBlueDot = isBlueDot
                });
            }

            return rsDataPoints;
        }

        private RSMetricsDto CalculateRSMetrics(List<CandlestickDto> stockData, List<RSDataPointDto> rsData, TradeDto trade)
        {
            // Get latest values
            var latestCandle = stockData.LastOrDefault();
            var latestRS = rsData.LastOrDefault();

            if (latestCandle == null || latestRS == null)
            {
                return new RSMetricsDto();
            }

            // Calculate SMAs for trend analysis (Stage 2)
            var closes = stockData.Select(c => c.Close).ToList();
            var sma50Values = CalculateSMA(closes, 50);
            var sma150Values = CalculateSMA(closes, 150);
            var sma200Values = CalculateSMA(closes, 200);

            double sma50 = closes.Count >= 50 ? sma50Values.Last() : 0;
            double sma150 = closes.Count >= 150 ? sma150Values.Last() : 0;
            double sma200 = closes.Count >= 200 ? sma200Values.Last() : 0;

            // Check Stage 2 trend: Close > SMA50 > SMA150 > SMA200
            bool isStage2Trend = sma50 > 0 && sma150 > 0 && sma200 > 0 &&
                                 latestCandle.Close > sma50 &&
                                 sma50 > sma150 &&
                                 sma150 > sma200;

            // Calculate distance from 52-week high
            int lookback252 = Math.Min(252, stockData.Count);
            double high52Week = stockData.TakeLast(lookback252).Max(c => c.High);
            double distanceFrom52WeekHigh = ((high52Week - latestCandle.Close) / high52Week) * 100;

            return new RSMetricsDto
            {
                // Institutional data would come from external API - stub for now
                InstitutionalCount = null,
                InstitutionalPercent = null,
                InstitutionalCountDelta = null,
                IsInstitutionalGrowing = false,

                IsRSNewHigh = latestRS.IsRS50High,
                IsStage2Trend = isStage2Trend,
                DistanceFrom52WeekHigh = distanceFrom52WeekHigh,
                SMA50 = sma50,
                SMA150 = sma150,
                SMA200 = sma200
            };
        }

        private List<double> CalculateSMA(List<double> values, int period)
        {
            var smaValues = new List<double>();

            for (int i = 0; i < values.Count; i++)
            {
                if (i < period - 1)
                {
                    // Not enough data yet, use current value or 0
                    smaValues.Add(0);
                }
                else
                {
                    double sum = 0;
                    for (int j = 0; j < period; j++)
                    {
                        sum += values[i - j];
                    }
                    smaValues.Add(sum / period);
                }
            }

            return smaValues;
        }
    }
}
