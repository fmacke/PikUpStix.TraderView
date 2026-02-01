using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using IKBR_Report_Puller.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace IKBR_Report_Puller.Services
{
    public class TimeSeriesService : ITimeSeriesService
    {
        private readonly HttpClient _httpClient;
        private readonly string _connectionString;

        public TimeSeriesService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _connectionString = $"Server={configuration["Database:Host"]};Database={configuration["Database:DbName"]};User Id={configuration["Database:User"]};Password={configuration["Database:Password"]};TrustServerCertificate=True;";
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
                        string responseData = await response.Content.ReadAsStringAsync();

                        // Parse the response data
                        dynamic parsedData = Newtonsoft.Json.JsonConvert.DeserializeObject(responseData);
                        var result = parsedData?.chart?.result?[0];
                        if (result == null || result.indicators?.quote?[0] == null)
                        {
                            Console.WriteLine("No valid time series data found.");
                            return responseData;
                        }

                        var timestamps = result.timestamp;
                        var quotes = result.indicators.quote[0];

                        if (timestamps == null || quotes.open == null)
                        {
                            Console.WriteLine("Incomplete time series data.");
                            return responseData;
                        }

                        // Retrieve InstrumentId from dbo.Instruments
                        int? instrumentId = null;
                        using (var connection = new SqlConnection(_connectionString))
                        {
                            connection.Open();
                            using (var command = new SqlCommand(
                                "SELECT Id FROM dbo.Instruments WHERE InstrumentName = @instrumentName AND Provider = @provider AND Frequency = @frequency AND ListingExchange = @listingExchange", connection))
                            {
                                command.Parameters.AddWithValue("@instrumentName", ticker);
                                command.Parameters.AddWithValue("@provider", "YahooFinance");
                                command.Parameters.AddWithValue("@frequency", period);
                                command.Parameters.AddWithValue("@listingExchange", listingExchange);

                                instrumentId = (int?)command.ExecuteScalar();
                            }
                        }

                        if (instrumentId == null)
                        {
                            Console.WriteLine("Instrument not found in the database.");
                            return responseData;
                        }

                        // Insert time series data into dbo.HistoricalData
                        using (var connection = new SqlConnection(_connectionString))
                        {
                            connection.Open();
                            foreach (var i in Enumerable.Range(0, timestamps.Count))
                            {
                                using (var command = new SqlCommand(
                                    "INSERT INTO dbo.HistoricalData (Date, OpenPrice, ClosePrice, LowPrice, HighPrice, Volume, Settle, OpenInterest, InstrumentId) VALUES (@date, @openPrice, @closePrice, @lowPrice, @highPrice, @volume, @settle, @openInterest, @instrumentId)", connection))
                                {
                                    command.Parameters.AddWithValue("@date", DateTimeOffset.FromUnixTimeSeconds((long)timestamps[i]).DateTime);
                                    command.Parameters.AddWithValue("@openPrice", (double?)quotes.open[i] ?? (object)DBNull.Value);
                                    command.Parameters.AddWithValue("@closePrice", (double?)quotes.close[i] ?? (object)DBNull.Value);
                                    command.Parameters.AddWithValue("@lowPrice", (double?)quotes.low[i] ?? (object)DBNull.Value);
                                    command.Parameters.AddWithValue("@highPrice", (double?)quotes.high[i] ?? (object)DBNull.Value);
                                    command.Parameters.AddWithValue("@volume", (double?)quotes.volume[i] ?? (object)DBNull.Value);
                                    command.Parameters.AddWithValue("@settle", DBNull.Value); // Assuming settle is not provided
                                    command.Parameters.AddWithValue("@openInterest", DBNull.Value); // Assuming open interest is not provided
                                    command.Parameters.AddWithValue("@instrumentId", instrumentId);

                                    command.ExecuteNonQuery();
                                }
                            }
                        }

                        return responseData;
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