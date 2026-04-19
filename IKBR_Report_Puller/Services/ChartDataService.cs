using IBApi;
using IKBR_Report_Puller.IKBR;
using IKBR_Report_Puller.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IKBR_Report_Puller.Services
{
    /// <summary>
    /// Inherits from DefaultWrapper to hide the 60+ unused IBKR API methods.
    /// </summary>
    public class ChartDataService : DefaultWrapper, IChartDataService, IDisposable
    {
        private readonly EClientSocket _client;
        private readonly EReaderSignal _signal;
        private int _requestId = 0;

        // Connection Handshake Management
        private TaskCompletionSource<bool> _connectionTcs;

        // Data Request Management
        private TaskCompletionSource<List<Bar>> _dataTcs;
        private List<Bar> _currentBars;

        public bool IsConnected => _client.IsConnected();

        public ChartDataService(IConfiguration config)
        {
            _signal = new EReaderMonitorSignal();
            _client = new EClientSocket(this, _signal);
            var socketUrl = config["IBKRClient:SocketUrl"];
            var port = int.Parse(config["IBKRClient:Port"]);
            var clientId = int.Parse(config["IBKRClient:ClientId"]);
            ConnectAsync(socketUrl, port, clientId);
        }

        /// <summary>
        /// Establishes the TCP connection and waits for the NextValidId handshake.
        /// Prevents Error 504 (Not Connected).
        /// </summary>
        public async Task<bool> ConnectAsync(string host, int port, int clientId)
        {
            if (_client.IsConnected()) return true;

            _connectionTcs = new TaskCompletionSource<bool>();

            Console.WriteLine($"[Socket] Connecting to {host}:{port} with ClientID {clientId}...");
            _client.eConnect(host, port, clientId);

            // Start the background message processor (EReader)
            var reader = new EReader(_client, _signal);
            reader.Start();
            new Thread(() => {
                while (_client.IsConnected())
                {
                    _signal.waitForSignal();
                    reader.processMsgs();
                }
            })
            { IsBackground = true }.Start();

            // Wait for nextValidId callback or timeout after 5 seconds
            var completedTask = await Task.WhenAny(_connectionTcs.Task, Task.Delay(5000));
            return completedTask == _connectionTcs.Task && _client.IsConnected();
        }

        /// <summary>
        /// Requests historical data by contract ID and date range.
        /// </summary>
        public async Task<List<Bar>> GetHistoricalDataAsync(string conid, DateTime from, DateTime to, string symbol = null)
        {
            if (!_client.IsConnected())
                throw new InvalidOperationException("Socket not connected. Call ConnectAsync first.");

            _dataTcs = new TaskCompletionSource<List<Bar>>();
            _currentBars = new List<Bar>();
            int reqId = Interlocked.Increment(ref _requestId);

            // Determine if this is a forex pair by checking the symbol format (e.g., EUR.GBP, GBP.USD)
            // A valid forex pair has exactly two 3-letter currency codes separated by a dot
            bool isForex = IsForexPair(symbol);

            // Define the contract using the unique Conid
            Contract contract = new Contract
            {
                ConId = int.Parse(conid)
            };

            // Set contract-specific parameters based on instrument type
            string whatToShow;
            int useRTH;

            if (isForex)
            {
                // For forex, we need to specify the currency pair properly
                var currencies = symbol.Split('.');
                contract.Symbol = currencies[0];      // Base currency (e.g., EUR)
                contract.Currency = currencies[1];    // Quote currency (e.g., GBP)
                contract.SecType = "CASH";
                contract.Exchange = "IDEALPRO"; // Forex exchange
                whatToShow = "BID_ASK"; // Use BID_ASK for forex (MIDPOINT may not be available for historical data)
                useRTH = 0; // Include all trading hours (forex trades 24/5)
            }
            else
            {
                // Stock or other equity
                contract.SecType = "STK"; // Explicitly set security type for stocks
                contract.Exchange = "SMART";
                whatToShow = "TRADES";
                useRTH = 1; // Regular Trading Hours only
            }

            // Calculate duration string based on the from/to dates
            // IBKR requires a specific format: "3600 S", "3 D", "1 W", "1 M", "1 Y"
            string duration = CalculateIbkrDuration(from, to);

            // Format the endDateTime (to) as "yyyyMMdd-HH:mm:ss"
            string endDateTime = to.ToString("yyyyMMdd-HH:mm:ss");

            if (isForex)
            {
                Console.WriteLine($"[ChartDataService] Requesting historical data - Symbol: {contract.Symbol}/{contract.Currency}, ConId: {conid}, SecType: {contract.SecType}, Exchange: {contract.Exchange}, Duration: {duration}, WhatToShow: {whatToShow}");
            }
            else
            {
                Console.WriteLine($"[ChartDataService] Requesting historical data - Symbol: {symbol}, ConId: {conid}, SecType: {contract.SecType ?? "default"}, Exchange: {contract.Exchange}, Duration: {duration}, WhatToShow: {whatToShow}");
            }

            // Parameters: id, contract, endDateTime, duration, barSize, whatToShow, useRTH, formatDate, keepUpToDate, chartOptions
            _client.reqHistoricalData(
                reqId,
                contract,
                endDateTime,
                duration,
                "1 day",
                whatToShow,
                useRTH,
                1, // formatDate: 1 = yyyyMMdd HH:mm:ss
                false,
                null
            );

            // Wait for the response with a timeout (60 seconds for large historical data requests)
            var completedTask = await Task.WhenAny(_dataTcs.Task, Task.Delay(10000));

            if (completedTask != _dataTcs.Task)
            {
                Console.WriteLine($"[ChartDataService] Request timed out after 60 seconds for {symbol}");
                throw new TimeoutException($"Historical data request timed out for {symbol} (ConId: {conid})");
            }

            return await _dataTcs.Task;
        }

        private string CalculateIbkrDuration(DateTime from, DateTime to)
        {
            if (from >= to)
                throw new ArgumentException("The 'from' date must be earlier than the 'to' date.");

            TimeSpan span = to - from;

            // IBKR requires durations > 365 days to be specified in years
            if (span.TotalDays >= 365)
            {
                // Calculate years and round up to ensure the 'from' date is included
                // Use a more accurate calculation: divide by 365.25 to account for leap years
                double years = span.TotalDays / 365.25;
                int roundedYears = (int)Math.Ceiling(years);
                return $"{roundedYears} Y";
            }
            // Use Days if the span is at least 1 day but less than 365 days
            else if (span.TotalDays >= 1)
            {
                // We round up to the nearest whole day to ensure the 'from' date is included
                int days = (int)Math.Ceiling(span.TotalDays);
                return $"{days} D";
            }
            else
            {
                // For intra-day spans, use Seconds
                int seconds = (int)Math.Ceiling(span.TotalSeconds);
                return $"{seconds} S";
            }
        }

        /// <summary>
        /// Determines if a symbol represents a forex pair (e.g., EUR.GBP, USD.JPY)
        /// A valid forex pair has exactly two 3-letter currency codes separated by a dot
        /// </summary>
        private bool IsForexPair(string symbol)
        {
            if (string.IsNullOrEmpty(symbol) || !symbol.Contains('.'))
                return false;

            var parts = symbol.Split('.');

            // Must have exactly 2 parts
            if (parts.Length != 2)
                return false;

            // Both parts must be exactly 3 characters (standard currency code length)
            // and consist only of letters
            return parts[0].Length == 3 && parts[1].Length == 3 &&
                   parts[0].All(char.IsLetter) && parts[1].All(char.IsLetter);
        }

        // --- IBKR CALLBACK OVERRIDES ---

        public override void nextValidId(int orderId)
        {
            // Handshake completed successfully
            _connectionTcs?.TrySetResult(true);
        }

        public override void historicalData(int reqId, Bar bar)
        {
            // Fired for every OHLCV bar returned
            Console.WriteLine($"[ChartDataService] Received bar for reqId {reqId}: {bar.Time} O:{bar.Open} H:{bar.High} L:{bar.Low} C:{bar.Close}");
            _currentBars.Add(bar);
        }

        public override void historicalDataEnd(int reqId, string start, string end)
        {
            // All requested bars have been received
            Console.WriteLine($"[ChartDataService] Historical data complete for reqId {reqId}. Received {_currentBars.Count} bars from {start} to {end}");
            _dataTcs?.TrySetResult(_currentBars);
        }

        public override void error(int id, int errorCode, string errorMsg)
        {
            // IBKR warning codes (non-critical)
            if (IsWarningCode(errorCode))
            {
                Console.WriteLine($"[IBKR Warning {errorCode}] {errorMsg}");
                return; // Don't fail the operation for warnings
            }

            // Log critical errors
            Console.WriteLine($"[IBKR Error {errorCode}] {errorMsg}");

            // If this error is for a specific request (id > 0), fail the pending data request
            if (id > 0 && _dataTcs != null && !_dataTcs.Task.IsCompleted)
            {
                _dataTcs.TrySetException(new Exception($"IBKR Error {errorCode}: {errorMsg}"));
                return;
            }

            // If connection fails during handshake
            if (id == -1 && !_client.IsConnected())
            {
                _connectionTcs?.TrySetResult(false);
            }
        }

        /// <summary>
        /// Determines if an IBKR error code is a warning (non-critical) rather than a critical error
        /// </summary>
        private bool IsWarningCode(int errorCode)
        {
            // Common IBKR warning codes that don't require failing the operation
            return errorCode switch
            {
                2104 => true, // Market data farm connection is OK
                2106 => true, // HMDS data farm connection is OK
                2158 => true, // Sec-def data farm connection is OK
                2176 => true, // API version does not support fractional shares (auto-trimmed)
                2177 => true, // API version does not support some feature
                10285 => true, // API version does not support fractional size rules
                _ => false
            };
        }

        public void Dispose()
        {
            if (_client.IsConnected()) _client.eDisconnect();
        }
    }
}