using Microsoft.EntityFrameworkCore;
using TraderView.Application.UseCases.Backtest;
using TraderView.Domain.Entities;
using TraderView.Infrastructure.DbContexts;

namespace TraderView.Infrastructure.Services.Backtest
{
    /// <summary>
    /// Fetches historical bars from the database with caching to improve performance.
    /// </summary>
    public class HistoricalBarProvider : IHistoricalBarProvider
    {
        private readonly AppDbContext _dbContext;
        private readonly Dictionary<(int instrumentId, DateTime startDate, DateTime endDate), List<Bar>> _cache;

        public HistoricalBarProvider(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _cache = new Dictionary<(int, DateTime, DateTime), List<Bar>>();
        }

        /// <summary>
        /// Get bars for the given instrument and date range.
        /// Results are cached to avoid repeated database queries.
        /// </summary>
        public async Task<List<Bar>> GetBarsAsync(int instrumentId, DateTime startDate, DateTime endDate)
        {
            var cacheKey = (instrumentId, startDate, endDate);

            // Check cache first
            if (_cache.TryGetValue(cacheKey, out var cachedBars))
            {
                return cachedBars;
            }

            // Query database: use HistoricalDatum as the source
            // Assuming HistoricalDatum contains OHLCV data similar to Bar
            var historicalData = await _dbContext.HistoricalData
                .Where(h => h.InstrumentId == instrumentId &&
                            h.Date >= startDate &&
                            h.Date <= endDate)
                .OrderBy(h => h.Date)
                .ToListAsync();

            // Convert HistoricalDatum to Bar
            var bars = historicalData
                .Select(h => ConvertToBars(h))
                .Where(b => b != null)
                .Cast<Bar>()
                .ToList();

            // Cache the result
            _cache[cacheKey] = bars;

            return bars;
        }

        /// <summary>
        /// Convert HistoricalDatum to Bar.
        /// HistoricalDatum and Bar have matching schema (OpenPrice, ClosePrice, etc.)
        /// </summary>
        private Bar? ConvertToBars(HistoricalDatum data)
        {
            try
            {
                return new Bar
                {
                    Date = data.Date,
                    OpenPrice = data.OpenPrice,
                    ClosePrice = data.ClosePrice,
                    LowPrice = data.LowPrice,
                    HighPrice = data.HighPrice,
                    Volume = data.Volume,
                    InstrumentId = data.InstrumentId,
                    Settle = data.Settle ?? 0,
                    OpenInterest = data.OpenInterest ?? 0,
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Clear cache (optional, for explicit memory management).
        /// </summary>
        public void ClearCache()
        {
            _cache.Clear();
        }
    }
}
