using PikUpStix.TraderView.Interfaces;

namespace IKBR_Report_Puller.Console
{
    public class Application
    {
        private readonly IReportRunnerService _reportRunnerService;

        public Application(
            IReportRunnerService reportRunnerService)
        {
            _reportRunnerService = reportRunnerService;
        }


        public async Task RunAsync()
        {
            await _reportRunnerService.RunReportAsync(true, true);
        }
    }
}
