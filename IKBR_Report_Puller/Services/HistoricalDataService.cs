using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities;

namespace TraderView.Application.Services
{
    public class HistoricalDataService : IHistoricalDataService
    {
        private readonly IHistoricalDataRepository _repo;
        private const int SpxInstrumentId = 1002; // move to configuration

        public HistoricalDataService(IHistoricalDataRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public async Task<List<Bar>> GetCandlesticksAsync(int instrumentId, DateTime startDate, DateTime endDate)
        {
            return await _repo.GetCandlesticksAsync(instrumentId, startDate, endDate);
        }

        public async Task<int?> GetInstrumentIdBySymbolAsync(string symbol)
        {
            return await _repo.GetInstrumentIdBySymbolAsync(symbol);
        }
        public async Task<string> DetermineRegimeAsync(DateTime openDate, CancellationToken cancellationToken)
        {
            // Request enough history to compute 200-day SMA; repository returns candles for the range.
            var start = openDate.AddDays(-300);
            var candles = await _repo.GetCandlesticksAsync(SpxInstrumentId, start, openDate);

            var ordered = candles.OrderBy(c => c.Date).ToList();
            if (ordered.Count < 200) return "Unknown";

            var currentClose = Convert.ToDecimal(ordered.Last().ClosePrice);

            // Guard against insufficient subranges
            var last50 = ordered.Skip(Math.Max(0, ordered.Count - 50)).Select(c => c.ClosePrice);
            var last200 = ordered.Skip(Math.Max(0, ordered.Count - 200)).Select(c => c.ClosePrice);

            var sma50 = last50.Any() ? Convert.ToDecimal(last50.Average()) : 0m;
            var sma200 = last200.Any() ? Convert.ToDecimal(last200.Average()) : 0m;

            if (currentClose > sma50 && sma50 > sma200)
                return "Bull";
            if (currentClose < sma200)
                return "Correction";
            return "Choppy/Pressure";
        }
    }
}
