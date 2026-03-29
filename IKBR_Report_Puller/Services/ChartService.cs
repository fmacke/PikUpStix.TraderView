using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IBApi;
using IKBR_Report_Puller.Interfaces;

namespace IKBR_Report_Puller.Services
{
    /// <summary>
    /// Inherits from DefaultWrapper to hide the 60+ unused IBKR API methods.
    /// </summary>
    public class ChartService : DefaultWrapper, IChartService, IDisposable
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

        public ChartService()
        {
            _signal = new EReaderMonitorSignal();
            _client = new EClientSocket(this, _signal);
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
        /// Requests historical data and returns a Task that completes when all bars are received.
        /// Perfect for CAN SLIM trend analysis.
        /// </summary>
        public async Task<List<Bar>> GetHistoricalDataAsync(string symbol)
        {
            if (!_client.IsConnected())
                throw new InvalidOperationException("Socket not connected. Call ConnectAsync first.");

            _dataTcs = new TaskCompletionSource<List<Bar>>();
            _currentBars = new List<Bar>();
            int reqId = Interlocked.Increment(ref _requestId);

            Contract contract = new Contract
            {
                Symbol = symbol,
                SecType = "STK",
                Currency = "USD",
                Exchange = "SMART"
            };

            // Requesting 1 Year of Daily Bars for Relative Strength Calculation
            // Parameters: id, contract, endDateTime, duration, barSize, whatToShow, useRTH, formatDate, keepUpToDate, chartOptions
            _client.reqHistoricalData(reqId, contract, "", "1 Y", "1 day", "TRADES", 1, 1, false, null);

            return await _dataTcs.Task;
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