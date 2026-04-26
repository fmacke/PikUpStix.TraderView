using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using IKBR_Report_Puller.Domain;
using IKBR_Report_Puller.IKBR;
using IKBR_Report_Puller.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;

namespace IKBR_Report_Puller.Services
{
    public class HistoricalDataService : IHistoricalDataService
    {
        private readonly IChartDataService _chartDataService;
        private readonly IHistoricalDataRepository _historicalDataRepository;
        private readonly IInstrumentRepository _instrumentRepository;
        private readonly ITradeHistoryReportService _tradeHistoryReportService;
        private readonly IConfiguration _config;

        public HistoricalDataService(
            IChartDataService chartDataService,
            IHistoricalDataRepository historicalDataRepository,
            IInstrumentRepository instrumentRepository,
            ITradeHistoryReportService tradeHistoryReportService,
            IConfiguration config)
        {
            _chartDataService = chartDataService;
            _historicalDataRepository = historicalDataRepository;
            _instrumentRepository = instrumentRepository;
            _tradeHistoryReportService = tradeHistoryReportService;
            _config = config;
        }

        /// <summary>
        /// Updates historical data for all positions/trades by identifying and filling missing date ranges
        /// </summary>
        public void UpdateHistoricalDataForHistoricalTrades(List<HistoricalTrade> trades)
        {
            foreach (var trade in trades)
            {
                Console.WriteLine($"Processing trade for {trade.Symbol} opened on {trade.TradeOpened:yyyy-MM-dd} and closed on {trade.TradeClosed:yyyy-MM-dd}");
                List<(DateTime startDate, DateTime endDate)> missingRanges = GetMissingRanges(trade.TradeOpened, trade.TradeClosed, trade.Symbol, trade.InstrumentId);

                if (missingRanges.Any())
                {
                    Console.WriteLine($"Found {missingRanges.Count} missing date range(s) for {trade.Symbol}:");
                    foreach (var range in missingRanges)
                    {
                        Console.WriteLine($"  Missing data from {range.startDate:yyyy-MM-dd} to {range.endDate:yyyy-MM-dd}");
                        var domainBars =FetchHistoricalData(trade.SecurityId, trade.Symbol, range.startDate, range.endDate, trade.InstrumentId);
                        // Save to database
                        _historicalDataRepository.UpdateHistoricalData(trade.InstrumentId.ToString(), domainBars.Result);
                        Console.WriteLine($"Successfully saved {domainBars.Result.Count} bars for {trade.Symbol}");
                    }
                }
                else
                {
                    Console.WriteLine($"All required historical data exists for {trade.Symbol}");
                }
            }
        }

        public void UpdateHistoricalDataForPositions(List<OpenPosition> positions)
        {
            foreach (var position in positions)
            {
                Console.WriteLine($"Processing position for {position.Symbol} opened on {position.OpenDateTime:yyyy-MM-dd}");

                var instrumentId = _instrumentRepository.GetInstrumentIdFromConId(position.Conid.ToString());

                if(instrumentId == null)
                {
                    Console.WriteLine($"Warning: Could not find instrument ID for {position.Symbol} (conid: {position.Conid}). Adding new instrument.");
                    instrumentId = _instrumentRepository.InsertInstrument(position.Conid.ToString(), position.Symbol, position.ListingExchange, position.Currency);
                }
                if (instrumentId != null)
                {
                    List<(DateTime startDate, DateTime endDate)> missingRanges = GetMissingRanges(
                        Convert.ToDateTime(position.OpenDateTime), Convert.ToDateTime(position.OpenDateTime),
                        position.Symbol, Convert.ToInt32(instrumentId));

                    if (missingRanges.Any())
                    {
                        Console.WriteLine($"Found {missingRanges.Count} missing date range(s) for {position.Symbol}:");
                        foreach (var range in missingRanges)
                        {
                            Console.WriteLine($"  Missing data from {range.startDate:yyyy-MM-dd} to {range.endDate:yyyy-MM-dd}");
                            var domainBars = FetchHistoricalData(position.Conid.ToString(), position.Symbol, range.startDate, range.endDate, Convert.ToInt32(instrumentId));
                            // Save to database
                            _historicalDataRepository.UpdateHistoricalData(instrumentId.ToString(), domainBars.Result);
                            Console.WriteLine($"Successfully saved {domainBars.Result.Count} bars for {position.Symbol}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"All required historical data exists for {position.Symbol}");
                    }
                }
            }
        }

        private List<(DateTime startDate, DateTime endDate)> GetMissingRanges(DateTime tradeOpened, DateTime tradeClosed, string symbol, int instrumentId)
        {
            // Calculate date range: startDate - 100 days to endDate + 100 days
            DateTime requiredStartDate = tradeOpened.AddDays(-100);
            DateTime requiredEndDate = tradeClosed.AddDays(100);

            // If endDate + 100 is past today, use today instead
            DateTime today = DateTime.Today;
            if (requiredEndDate > today)
            {
                requiredEndDate = today;
            }

            Console.WriteLine($"Checking historical data for {symbol} from {requiredStartDate:yyyy-MM-dd} to {requiredEndDate:yyyy-MM-dd}");

            // Check for missing date ranges
            var missingRanges = _historicalDataRepository.GetMissingDateRanges(instrumentId, requiredStartDate, requiredEndDate);
            return missingRanges;
        }

        /// <summary>
        /// Fetches historical data from IBKR and saves it to the database
        /// </summary>
        private async Task<List<Bar>> FetchHistoricalData(string conid, string symbol, DateTime startDate, DateTime endDate, int instrumentId)
        {
            try
            {
                if (startDate == endDate)
                {
                    startDate = startDate.AddDays(-1);
                }

                Console.WriteLine($"Fetching historical data for {symbol} (conid: {conid}) from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

                // Fetch data from IBKR API
                await _chartDataService.ConnectAsync(
                    _config["IBKRClient:SocketUrl"], 
                    int.Parse(_config["IBKRClient:Port"]), 
                    int.Parse(_config["IBKRClient:ClientId"]));

                var ibkrBars = await _chartDataService.GetHistoricalDataAsync(conid, startDate, endDate, symbol);

                if (ibkrBars == null || !ibkrBars.Any())
                {
                    Console.WriteLine($"Warning: No historical data returned for {symbol}");
                    return new List<Bar>();
                }

                // Convert IBKR bars to domain bars
                var domainBars = BarConverter.ConvertToDomainBars(ibkrBars, instrumentId);
                return domainBars;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching historical data for {symbol}: {ex.Message}");
                return new List<Bar>();
            }
        }
    }
}
