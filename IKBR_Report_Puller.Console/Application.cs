using TraderView.Application.Interfaces.Services;
using TraderView.Application.Models.FMP;

namespace TraderView.Console
{
    public class Application
    {
        private readonly IReportRunnerService _reportRunnerService;
        private readonly IMarketDataService _financialDataService;

        public Application(
            IReportRunnerService reportRunnerService,
            IMarketDataService financialDataService)
        {
            _reportRunnerService = reportRunnerService;
            _financialDataService = financialDataService;
        }


        public async Task RunAsync()
        {
            await _reportRunnerService.RunReportAsync(true, true);
            //var screenerList = await _financialDataService.RunScreenerAsync(new CanSlimScreenerCriteria());
            //foreach (var screener in screenerList)
            //{
            //    System.Console.WriteLine($"Symbol: {screener.Symbol}, Sector: {screener.Sector}");
            //}
            //var lethavealook = await _financialDataService.GetKeyMetricsTtmAsync("NVDA");
            //System.Console.WriteLine($"NVDA TTM Revenue:");
        }
    }
}
