using System.Net.Http;

public class IKBRTodayReportService : IKBRReportServiceBase
{
    private const string TodayQueryId = "1371134";

    public IKBRTodayReportService(string token, string baseUrl, HttpClient client)
        : base(token, TodayQueryId, baseUrl, client)
    {
    }
}
