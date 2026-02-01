using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using IKBR_Report_Puller.Interfaces;

namespace IKBR_Report_Puller.Services
{
    public class TimeSeriesService : ITimeSeriesService
    {
        private readonly HttpClient _httpClient;

        public TimeSeriesService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GetTimeSeriesDataAsync(string ticker, string listingExchange, DateTime startDate, DateTime endDate, string period)
        {
            var adjustedTicker = string.IsNullOrEmpty(listingExchange) ? ticker : $"{ticker}{GetListingExchange(listingExchange)}";
            var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{ticker}?period1={new DateTimeOffset(startDate).ToUnixTimeSeconds()}&period2={new DateTimeOffset(endDate).ToUnixTimeSeconds()}&interval={period}";

            int maxRetries = 1;
            int delay = 1000; // Initial delay in milliseconds

            for (int retry = 0; retry < maxRetries; retry++)
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                    var response = await _httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        Console.WriteLine("Rate limit exceeded. Retrying...");
                    }
                    else
                    {
                        response.EnsureSuccessStatusCode();
                    }
                }
                catch (HttpRequestException ex) when (retry < maxRetries - 1)
                {
                    Console.WriteLine($"Request failed: {ex.Message}. Retrying...");
                }

                await Task.Delay(delay);
                delay *= 2; // Exponential backoff
            }

            throw new Exception("Failed to fetch time series data after multiple retries.");
        }

        private string? GetListingExchange(string listingExchange)
        {
            return listingExchange switch
            {
                "NASDAQ" => "",
                "NYSE" => "",
                "AMEX" => "",
                "IBIS" => ".DE",
                "IBIS2" => ".DE",
                "LSEETF" => ".L",                
                "LSE" => ".L",
                "AEB" => ".AS",
                _ => string.Empty,
            };
        }
    }
}