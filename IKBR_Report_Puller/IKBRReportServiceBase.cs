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
        Console.WriteLine($"Request URL: {requestUrl}"); // Add logging

        string referenceCode = null;
        string statementUrl = null;

        // Retry logic for initial request (handles transient errors like 1001)
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                // Add null check and better error handling
                if (_client == null)
                {
                    throw new InvalidOperationException("HttpClient is null. Ensure it's properly configured in dependency injection.");
                }

                Console.WriteLine($"Sending request (attempt {attempt + 1}/{maxRetries})...");
                HttpResponseMessage initialResponse = await _client.GetAsync(requestUrl).ConfigureAwait(false);

                Console.WriteLine($"Response status code: {initialResponse.StatusCode}");
                initialResponse.EnsureSuccessStatusCode();

                string initialResponseBody = await initialResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

                Console.WriteLine("Initial API Response:");
                Console.WriteLine(initialResponseBody);

                XDocument initialXml = XDocument.Parse(initialResponseBody);
                var responseElement = initialXml.Element("FlexStatementResponse");
                string status = responseElement?.Element("Status")?.Value;
                referenceCode = responseElement?.Element("ReferenceCode")?.Value;
                statementUrl = responseElement?.Element("Url")?.Value;

                if (status == "Success" && !string.IsNullOrEmpty(referenceCode) && !string.IsNullOrEmpty(statementUrl))
                {
                    Console.WriteLine($"Report requested successfully. Reference code: {referenceCode}");
                    break;
                }

                // Handle transient errors (1001, 1003, 1018, etc.)
                string errorCode = responseElement?.Element("ErrorCode")?.Value;
                string errorMessage = responseElement?.Element("ErrorMessage")?.Value;

                // Known transient error codes that should be retried
                // 1001: Statement generation temporarily unavailable
                // 1003: Statement generation in progress
                // 1018: Service temporarily unavailable
                if (errorCode == "1001" || errorCode == "1003" || errorCode == "1018")
                {
                    Console.WriteLine($"Transient error {errorCode}: {errorMessage}");
                    if (attempt < maxRetries - 1)
                    {
                        // Use exponential backoff with jitter for better retry behavior
                        int waitTime = delayInSeconds * (int)Math.Pow(2, Math.Min(attempt, 4)); // Cap at 2^4 = 16x
                        int jitter = new Random().Next(0, 2000); // Add 0-2 seconds random jitter
                        int totalWaitMs = (waitTime * 1000) + jitter;

                        Console.WriteLine($"Retrying initial request in {totalWaitMs / 1000.0:F1} seconds... (Attempt {attempt + 1}/{maxRetries})");
                        await Task.Delay(totalWaitMs).ConfigureAwait(false);
                        continue;
                    }
                }

                // Non-transient error or final retry exhausted
                throw new InvalidOperationException($"Failed to request report. ErrorCode: {errorCode}, ErrorMessage: {errorMessage}");
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw our own exception
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP Request failed: {httpEx.Message}");
                if (attempt < maxRetries - 1)
                {
                    int waitTime = delayInSeconds * (int)Math.Pow(2, Math.Min(attempt, 4));
                    Console.WriteLine($"Retrying initial request in {waitTime} seconds... (Attempt {attempt + 1}/{maxRetries})");
                    await Task.Delay(TimeSpan.FromSeconds(waitTime)).ConfigureAwait(false);
                }
                else
                {
                    throw new InvalidOperationException($"Failed to request report after {maxRetries} retries due to HTTP errors.", httpEx);
                }
            }
            catch (TaskCanceledException tcEx)
            {
                Console.WriteLine($"Request timed out: {tcEx.Message}");
                if (attempt < maxRetries - 1)
                {
                    int waitTime = delayInSeconds * (int)Math.Pow(2, Math.Min(attempt, 4));
                    Console.WriteLine($"Retrying initial request in {waitTime} seconds... (Attempt {attempt + 1}/{maxRetries})");
                    await Task.Delay(TimeSpan.FromSeconds(waitTime)).ConfigureAwait(false);
                }
                else
                {
                    throw new TimeoutException($"Request timed out after {maxRetries} retries.", tcEx);
                }
            }
            catch (Exception ex) when (attempt < maxRetries - 1)
            {
                // Network errors or other exceptions - retry with exponential backoff
                int waitTime = delayInSeconds * (int)Math.Pow(2, Math.Min(attempt, 4));
                Console.WriteLine($"Request failed with exception: {ex.GetType().Name} - {ex.Message}");
                Console.WriteLine($"Retrying initial request in {waitTime} seconds... (Attempt {attempt + 1}/{maxRetries})");
                await Task.Delay(TimeSpan.FromSeconds(waitTime)).ConfigureAwait(false);
            }
        }

        if (string.IsNullOrEmpty(referenceCode) || string.IsNullOrEmpty(statementUrl))
        {
            throw new InvalidOperationException("Failed to request report after multiple retries.");
        }

        for (int i = 0; i < maxRetries; i++)
        {
            Console.WriteLine($"Attempt {i + 1} of {maxRetries}: Fetching the full report in {delayInSeconds} seconds...");
            await Task.Delay(TimeSpan.FromSeconds(delayInSeconds)).ConfigureAwait(false);

            string getStatementUrl = $"{statementUrl}?t={_token}&q={referenceCode}&v=3";
            Console.WriteLine($"Fetching report from: {getStatementUrl}");

            try
            {
                HttpResponseMessage reportResponse = await _client.GetAsync(getStatementUrl).ConfigureAwait(false);

                // Check for 403 Forbidden - usually means report not ready yet
                if (reportResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    Console.WriteLine("Received 403 Forbidden - report may not be ready yet. Retrying...");
                    if (i < maxRetries - 1)
                    {
                        continue;
                    }
                    throw new InvalidOperationException("Received 403 Forbidden from IBKR API. The report may not be accessible or requires different credentials.");
                }

                reportResponse.EnsureSuccessStatusCode();
                string reportBody = await reportResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

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
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP request failed: {httpEx.Message}");
                if (i < maxRetries - 1)
                {
                    Console.WriteLine("Retrying...");
                    continue;
                }
                throw;
            }
        }

        throw new TimeoutException("Failed to retrieve the report after multiple retries.");
    }
}