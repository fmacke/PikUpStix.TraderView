namespace TraderView.Application.Interfaces.Services
{
    public interface IReportRunnerService
    {
        Task RunReportAsync(bool writeExcelReportToFolder, bool updateMarketData);
    }
}
