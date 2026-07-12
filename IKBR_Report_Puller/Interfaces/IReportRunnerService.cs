namespace PikUpStix.TraderView.Interfaces
{
    public interface IReportRunnerService
    {
        Task RunReportAsync(bool writeOutputToExcel);
    }
}
