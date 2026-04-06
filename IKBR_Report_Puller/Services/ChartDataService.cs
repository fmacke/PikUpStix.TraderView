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
        public async Task<List<Bar>> GetHistoricalDataAsync(string conid, DateTime from, DateTime to)
        {
            if (!_client.IsConnected())
                throw new InvalidOperationException("Socket not connected. Call ConnectAsync first.");

            _dataTcs = new TaskCompletionSource<List<Bar>>();
            _currentBars = new List<Bar>();
            int reqId = Interlocked.Increment(ref _requestId);

            // Define the contract using the unique Conid
            Contract contract = new Contract
            {
                ConId = int.Parse(conid),
                Exchange = "SMART" // Usually required to route the request effectively
            };

            // Calculate duration string based on the from/to dates
            // IBKR requires a specific format: "3600 S", "3 D", "1 W", "1 M", "1 Y"
            string duration = CalculateIbkrDuration(from, to);

            // Format the endDateTime (to) as "yyyyMMdd-HH:mm:ss"
            string endDateTime = to.ToString("yyyyMMdd-HH:mm:ss");

            // Parameters: id, contract, endDateTime, duration, barSize, whatToShow, useRTH, formatDate, keepUpToDate, chartOptions
            _client.reqHistoricalData(
                reqId,
                contract,
                endDateTime,
                duration,
                "1 day",
                "TRADES",
                1, // useRTH: 1 = Regular Trading Hours only (Crucial for CAN SLIM RS lines)
                1, // formatDate: 1 = yyyyMMdd HH:mm:ss
                false,
                null
            );

            return await _dataTcs.Task;
        }

        private string CalculateIbkrDuration(DateTime from, DateTime to)
        {
            if (from >= to)
                throw new ArgumentException("The 'from' date must be earlier than the 'to' date.");

            TimeSpan span = to - from;

            // Use Days if the span is at least 1 day. 
            // IBKR allows "D" for days.
            if (span.TotalDays >= 1)
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

        // --- IBKR CALLBACK OVERRIDES ---

        public override void nextValidId(int orderId)
        {
            // Handshake completed successfully
            _connectionTcs?.TrySetResult(true);
        }

        public override void historicalData(int reqId, Bar bar)
        {
            // Fired for every OHLCV bar returned
            _currentBars.Add(bar);
        }

        public override void historicalDataEnd(int reqId, string start, string end)
        {
            // All requested bars have been received
            _dataTcs?.TrySetResult(_currentBars);
        }

        public override void error(int id, int errorCode, string errorMsg)
        {
            // Log errors for debugging
            Console.WriteLine($"[IBKR Error {errorCode}] {errorMsg}");

            // Handle specific CAN SLIM issues
            if (errorCode == 162) // Pacing violation or no market data permission
            {
                _dataTcs?.TrySetException(new Exception($"Market Data Error: {errorMsg}"));
            }

            // If connection fails during handshake
            if (id == -1 && !_client.IsConnected())
            {
                _connectionTcs?.TrySetResult(false);
            }
        }

        public void Dispose()
        {
            if (_client.IsConnected()) _client.eDisconnect();
        }
    }
}