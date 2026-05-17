using IKBR_Report_Puller.Domain;
using IKBR_Report_Puller.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace IKBR_Report_Puller.Services
{
    /// <summary>
    /// Service for retrieving economic calendar data from Financial Modeling Prep API
    /// </summary>
    public class EconomicCalendarService : IEconomicCalendarService
    {
        private readonly HttpClient _httpClient;
        private readonly IEconomicCalendarRepository _repository;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _outputFilePath;

        public EconomicCalendarService(
            HttpClient httpClient,
            IEconomicCalendarRepository repository,
            string apiKey,
            string baseUrl,
            string outputFilePath)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            _outputFilePath = outputFilePath ?? throw new ArgumentNullException(nameof(outputFilePath));
        }

        /// <summary>
        /// Fetches economic calendar data from API, saves to file and database
        /// </summary>
        public async Task<List<EconomicCalendarEvent>> FetchAndSaveEconomicCalendarAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                // Build API URL with date parameters
                var fromDateStr = fromDate.ToString("yyyy-MM-dd");
                var toDateStr = toDate.ToString("yyyy-MM-dd");
                var url = $"{_baseUrl}/economic-calendar?from={fromDateStr}&to={toDateStr}&apikey={_apiKey}";

                Console.WriteLine($"Fetching economic calendar data from {fromDateStr} to {toDateStr}...");

                // Fetch data from API
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                // Deserialize JSON response
                var events = JsonSerializer.Deserialize<List<EconomicCalendarEvent>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (events == null || events.Count == 0)
                {
                    Console.WriteLine("No economic calendar events found for the specified date range.");
                    return new List<EconomicCalendarEvent>();
                }

                Console.WriteLine($"Retrieved {events.Count} economic calendar events.");

                // Save to file
                await SaveToFileAsync(events, fromDateStr, toDateStr);

                // Save to database
                _repository.UpsertEconomicCalendarEvents(events);

                return events;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP error fetching economic calendar: {ex.Message}");
                throw;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON deserialization error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching and saving economic calendar: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Saves economic calendar events to a JSON file
        /// </summary>
        private async Task SaveToFileAsync(List<EconomicCalendarEvent> events, string fromDate, string toDate)
        {
            try
            {
                // Ensure directory exists
                Directory.CreateDirectory(_outputFilePath);

                // Create filename with date range
                var fileName = $"EconomicCalendar_{fromDate}_to_{toDate}_{DateTime.UtcNow:yyyyMMddHHmmss}.json";
                var filePath = Path.Combine(_outputFilePath, fileName);

                // Serialize and save to file
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(events, jsonOptions);
                await File.WriteAllTextAsync(filePath, json);

                Console.WriteLine($"Economic calendar data saved to: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving economic calendar to file: {ex.Message}");
                throw;
            }
        }
    }
}
