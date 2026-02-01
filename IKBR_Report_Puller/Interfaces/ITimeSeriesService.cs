namespace IKBR_Report_Puller.Interfaces
{
    public interface ITimeSeriesService
    {
        Task<string> GetTimeSeriesDataAsync(string ticker, string listingExchange, DateTime startDate, DateTime endDate, string period);
    }
}