using TraderView.Application.Interfaces.Services;

namespace TraderView.Console
{
    public class Application
    {
        private readonly IReportRunnerService _reportRunnerService;
        private readonly IMarketDataService _financialDataService;
        private readonly IRiskMatrixService _riskMatrixService;
        private readonly ICanSlimScreenerService _canSlimScreenerService;

        public Application(
            IReportRunnerService reportRunnerService,
            IMarketDataService financialDataService,
            IRiskMatrixService riskMatrixService,
            ICanSlimScreenerService canSlimScreenerService)
        {
            _reportRunnerService = reportRunnerService;
            _financialDataService = financialDataService;
            _riskMatrixService = riskMatrixService;
            _canSlimScreenerService = canSlimScreenerService;
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
            // Win rates matching columns: 30%, 40%, 50%, and Custom (42.11%)
            //RunRiskMatrixTest();

            await Task.CompletedTask;
        }

        private void RunRiskMatrixTest()
        {
            decimal[] winRates = { 30.0m, 40.0m, 50.0m, 42.11m };
            const int tradesCount = 10;

            // (Gain%, Loss%) pairs matching the rows from your spreadsheet
            var scenarios = new List<(decimal Gain, decimal Loss)>
            {
                (4.00m, 2.00m),
                (6.00m, 3.00m),
                (8.00m, 4.00m),
                (12.00m, 6.00m),
                (14.00m, 7.00m),
                (16.00m, 8.00m),
                (20.00m, 10.00m),
                (24.00m, 12.00m),
                (30.00m, 15.00m),
                (36.00m, 18.00m),
                (42.00m, 21.00m),
                (48.00m, 24.00m),
                (54.00m, 27.00m),
                (60.00m, 30.00m),
                (70.00m, 35.00m),
                (80.00m, 40.00m),
                (90.00m, 45.00m),
                (100.00m, 50.00m)
            };

            // Print Header
            System.Console.WriteLine($"{"%Gain",-10} {"%Loss",-10} {"G/L Ratio",-12} {"30% Bat Av",-14} {"40% Bat Av",-14} {"50% Bat Av",-14} {"My Batting Av (42.11%)",-22}");
            System.Console.WriteLine(new string('-', 98));

            // Compute and display rows
            foreach (var (gain, loss) in scenarios)
            {
                decimal glRatio = loss == 0 ? 0 : (gain / loss) * 100m;

                var res30 = _riskMatrixService.CalculateExpectedRoi(gain, loss, 30.0m, tradesCount);
                var res40 = _riskMatrixService.CalculateExpectedRoi(gain, loss, 40.0m, tradesCount);
                var res50 = _riskMatrixService.CalculateExpectedRoi(gain, loss, 50.0m, tradesCount);
                var resCustom = _riskMatrixService.CalculateExpectedRoi(gain, loss, 42.11m, tradesCount);

                System.Console.WriteLine(
                    $"{gain,8:F2}% " +
                    $"{loss,8:F2}% " +
                    $"{glRatio,10:F2}% " +
                    $"{res30.CompoundedRoi,12:F2}% " +
                    $"{res40.CompoundedRoi,12:F2}% " +
                    $"{res50.CompoundedRoi,12:F2}% " +
                    $"{resCustom.CompoundedRoi,20:F2}%"
                );
            }
        }
    }
}
