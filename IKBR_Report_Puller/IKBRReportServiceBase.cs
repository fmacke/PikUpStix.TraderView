using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

public class IKBRReportServiceBase
{
    private readonly string _token;
    private readonly string _queryId;
    private readonly string _baseUrl;
    private readonly HttpClient _client;

    public IKBRReportServiceBase(string token, string queryId, string baseUrl, HttpClient client)
    {
        if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));
        if (string.IsNullOrEmpty(queryId)) throw new ArgumentNullException(nameof(queryId));
        if (string.IsNullOrEmpty(baseUrl)) throw new ArgumentNullException(nameof(baseUrl));

        _token = token;
        _queryId = queryId;
        _baseUrl = baseUrl;
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<XDocument> FetchReportAsync(int maxRetries = 10, int delayInSeconds = 15)
    {
        string requestUrl = $"{_baseUrl}?t={_token}&q={_queryId}&v=3";
        Console.WriteLine("Pinging Flex Query API to request report...");

        HttpResponseMessage initialResponse = await _client.GetAsync(requestUrl);
        initialResponse.EnsureSuccessStatusCode();
        string initialResponseBody = await initialResponse.Content.ReadAsStringAsync();

        Console.WriteLine("Initial API Response:");
        Console.WriteLine(initialResponseBody);

        XDocument initialXml = XDocument.Parse(initialResponseBody);
        var responseElement = initialXml.Element("FlexStatementResponse");
        string status = responseElement?.Element("Status")?.Value;
        string referenceCode = responseElement?.Element("ReferenceCode")?.Value;
        string statementUrl = responseElement?.Element("Url")?.Value;

        if (status != "Success" || string.IsNullOrEmpty(referenceCode) || string.IsNullOrEmpty(statementUrl))
        {
            string errorCode = responseElement?.Element("ErrorCode")?.Value;
            string errorMessage = responseElement?.Element("ErrorMessage")?.Value;
            throw new InvalidOperationException($"Failed to request report. ErrorCode: {errorCode}, ErrorMessage: {errorMessage}");
        }

        Console.WriteLine($"Report requested successfully. Reference code: {referenceCode}");

        for (int i = 0; i < maxRetries; i++)
        {
            Console.WriteLine($"Attempt {i + 1} of {maxRetries}: Fetching the full report in {delayInSeconds} seconds...");
            await Task.Delay(TimeSpan.FromSeconds(delayInSeconds));

            string getStatementUrl = $"{statementUrl}?t={_token}&q={referenceCode}&v=3";
            HttpResponseMessage reportResponse = await _client.GetAsync(getStatementUrl);
            reportResponse.EnsureSuccessStatusCode();
            string reportBody = await reportResponse.Content.ReadAsStringAsync();

            XDocument reportXml;
            try
            {
                reportXml = XDocument.Parse(reportBody);
            }
            catch (System.Xml.XmlException)
            {
                Console.WriteLine("Received non-XML response while waiting for the report. Retrying...");
                continue;
            }

            var flexStatementResponse = reportXml.Element("FlexStatementResponse");
            if (flexStatementResponse != null && flexStatementResponse.Element("ErrorCode")?.Value == "1019")
            {
                Console.WriteLine("Report generation in progress. Will try again.");
                continue;
            }

            if (reportXml.Element("FlexQueryResponse") != null)
            {
                Console.WriteLine("Full report received.");
                return reportXml;
            }

            Console.WriteLine("Received an unexpected response format. Retrying...");
        }

        throw new TimeoutException("Failed to retrieve the report after multiple retries.");
    }
}
