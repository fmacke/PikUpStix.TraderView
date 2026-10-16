using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;

namespace TraderView.Application.Services
{
    /// <summary>
    /// Service responsible for trading related reports. This implementation follows the application's
    /// repository/specification pattern by depending on repositories (no direct DbContext usage).
    /// It currently implements market regime detection used elsewhere in the application.
    /// </summary>
    public class TradingReportService : ITradingReportService
    {
        private readonly IHistoricalDataRepository _historicalDataRepo;
        private const int SpxInstrumentId = 1002; // consider moving to configuration in future

        public TradingReportService(IHistoricalDataRepository historicalDataRepo)
        {
            _historicalDataRepo = historicalDataRepo ?? throw new ArgumentNullException(nameof(historicalDataRepo));
        }


        public async Task<string> DetermineRegimeAsync(DateTime openDate, CancellationToken cancellationToken)
        {
            // Request enough history to compute 200-day SMA; repository returns candles for the range.
            var start = openDate.AddDays(-300);
            var candles = await _historicalDataRepo.GetCandlesticksAsync(SpxInstrumentId, start, openDate);

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
