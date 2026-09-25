using Microsoft.EntityFrameworkCore;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Specifications.HistoricalData;
using TraderView.Domain.Entities;
using TraderView.Infrastructure.DbContexts;

namespace TraderView.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for HistoricalData (chart data) operations using EF Core
    /// </summary>
    public class HistoricalDataRepository : EfBaseRepository<HistoricalDatum>, IHistoricalDataRepository
    {
        public HistoricalDataRepository(AppDbContext db) : base(db)
        {
        }

        /// <summary>
        /// Inserts chart data bars for a given instrument, skipping duplicates
        /// </summary>
        public async Task UpdateHistoricalDataAsync(string instrumentId, List<Bar> bars)
        {
            if (bars == null || !bars.Any())
            {
                Console.WriteLine("No bars data to insert.");
                return;
            }

            if (!int.TryParse(instrumentId, out int instrumentIdInt))
            {
                Console.WriteLine($"Invalid instrument ID: {instrumentId}");
                return;
            }

            // Get existing dates for this instrument
            var existingDates = await _db.Set<HistoricalDatum>()
                .Where(hd => hd.InstrumentId == instrumentIdInt)
                .Select(hd => hd.Date)
                .ToListAsync();

            var existingDateSet = new HashSet<DateTime>(existingDates);
            var newBars = bars.Where(bar => !existingDateSet.Contains(bar.Date)).ToList();

            if (!newBars.Any())
            {
                Console.WriteLine($"All chart data already exists for instrument {instrumentId}.");
                return;
            }

            // Convert Bar DTOs to HistoricalDatum entities
            var historicalData = newBars.Select(bar => new HistoricalDatum
            {
                Date = bar.Date,
                OpenPrice = bar.OpenPrice,
                ClosePrice = bar.ClosePrice,
                LowPrice = bar.LowPrice,
                HighPrice = bar.HighPrice,
                Volume = bar.Volume,
                Settle = bar.Settle,
                OpenInterest = bar.OpenInterest,
                InstrumentId = instrumentIdInt
            }).ToList();

            await AddRangeAsync(historicalData);
            Console.WriteLine($"Successfully inserted {newBars.Count} new chart data records for instrument {instrumentId}.");
        }

        /// <summary>
        /// Inserts chart data bars for a given instrument, skipping duplicates (legacy synchronous method)
        /// </summary>
        public void UpdateHistoricalData(string instrumentId, List<Bar> bars)
        {
            UpdateHistoricalDataAsync(instrumentId, bars).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets missing date ranges for historical data for a given instrument and date range
        /// </summary>
        public async Task<List<(DateTime startDate, DateTime endDate)>> GetMissingDateRangesAsync(int instrumentId, DateTime startDate, DateTime endDate)
        {
            var existingDates = await _db.Set<HistoricalDatum>()
                .Where(hd => hd.InstrumentId == instrumentId)
                .Select(hd => hd.Date)
                .ToListAsync();

            var missingRanges = new List<(DateTime startDate, DateTime endDate)>();

            if (!existingDates.Any())
            {
                // No data exists, return the entire range
                var adjustedEndDate = startDate == endDate ? endDate.AddDays(1) : endDate;
                return new List<(DateTime, DateTime)> { (startDate, adjustedEndDate) };
            }

            var existingDateSet = new HashSet<DateTime>(existingDates.Select(d => d.Date));

            // Generate all expected dates (trading days approximation - all weekdays)
            var expectedDates = new List<DateTime>();
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                // Skip weekends (rough approximation - doesn't account for holidays)
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                {
                    expectedDates.Add(date.Date);
                }
            }

            // Find missing dates
            var missingDates = expectedDates.Where(d => !existingDateSet.Contains(d.Date)).OrderBy(d => d).ToList();

            if (!missingDates.Any())
            {
                // No missing dates found
                return missingRanges;
            }

            // Group consecutive missing dates into ranges
            DateTime? rangeStart = null;
            DateTime? rangeEnd = null;

            foreach (var date in missingDates)
            {
                if (rangeStart == null)
                {
                    // Start a new range
                    rangeStart = date;
                    rangeEnd = date;
                }
                else
                {
                    // Check if this date is consecutive to the current range
                    var daysDiff = (date - rangeEnd.Value).Days;
                    bool isConsecutive = daysDiff == 1;

                    // Also consider weekends: Monday following Friday is consecutive
                    bool isWeekendGap = date.DayOfWeek == DayOfWeek.Monday && 
                                      rangeEnd.Value.DayOfWeek == DayOfWeek.Friday && 
                                      daysDiff <= 3;

                    if (isConsecutive || isWeekendGap)
                    {
                        // Extend the current range
                        rangeEnd = date;
                    }
                    else
                    {
                        // Gap detected, save current range and start new one
                        missingRanges.Add((rangeStart.Value, rangeEnd.Value));
                        rangeStart = date;
                        rangeEnd = date;
                    }
                }
            }

            // Add the final range
            if (rangeStart.HasValue && rangeEnd.HasValue)
            {
                missingRanges.Add((rangeStart.Value, rangeEnd.Value));
            }

            return missingRanges;
        }

        /// <summary>
        /// Gets missing date ranges for historical data for a given instrument and date range (legacy synchronous method)
        /// </summary>
        public List<(DateTime startDate, DateTime endDate)> GetMissingDateRanges(int instrumentId, DateTime startDate, DateTime endDate)
        {
            return GetMissingDateRangesAsync(instrumentId, startDate, endDate).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets candlestick data for a given instrument and date range asynchronously
        /// </summary>
        public async Task<List<Bar>> GetCandlesticksAsync(int instrumentId, DateTime startDate, DateTime endDate)
        {
            var specification = new CandleSticksByInstrumentAndDateRangeSpecification(instrumentId, startDate, endDate);
            var candleSticks = await GetAsync(specification);

            return candleSticks.Select(hd => new Bar
            {
                Date = hd.Date,
                OpenPrice = hd.OpenPrice,
                HighPrice = hd.HighPrice,
                LowPrice = hd.LowPrice,
                ClosePrice = hd.ClosePrice,
                Volume = hd.Volume,
                Settle = hd.Settle ?? 0,
                OpenInterest = hd.OpenInterest ?? 0,
                InstrumentId = hd.InstrumentId
            }).ToList();
        }

        /// <summary>
        /// Gets instrument ID by symbol name asynchronously
        /// </summary>
        public async Task<int?> GetInstrumentIdBySymbolAsync(string symbol)
        {
            var instrument = await _db.Set<Instrument>()
                .FirstOrDefaultAsync(i => i.InstrumentName == symbol);
            return instrument?.Id;
        }
    }
}
