using System.Xml.Linq;
using IKBR_Report_Puller.Domain;
using PikUpStix.TraderView.Interfaces;
using IKBR_Report_Puller.Services;
using Microsoft.Extensions.Configuration;

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
            await _reportRunnerService.RunReportAsync(true);
        }
    }
}
