using System;
using System.Collections.Generic;
using System.Linq;
using IKBR_Report_Puller.Data.Repositories;
using IKBR_Report_Puller.Domain;
using IKBR_Report_Puller.Interfaces;
using Microsoft.Extensions.Configuration;

namespace IKBR_Report_Puller.Services
{
    /// <summary>
    /// Coordinates database operations across multiple repositories
    /// Implements the Unit of Work pattern
    /// </summary>
    public class DataService : IDataService
    {
        private readonly string _connectionString;

        // Repositories
        private readonly TradeExecutionRepository _tradeExecutionRepository;
        private readonly OpenPositionRepository _openPositionRepository;
        private readonly HistoricalDataRepository _historicalDataRepository;
        private readonly InstrumentRepository _instrumentRepository;

        public DataService(IConfiguration config)
        {
            var dbUser = config["Database:User"];
            var dbPassword = config["Database:Password"];
            var dbHost = config["Database:Host"];
            var dbName = config["Database:DbName"];
            _connectionString = $"Server={dbHost};Database={dbName};User ID={dbUser};Password={dbPassword};TrustServerCertificate=True;";

            // Initialize repositories - InstrumentRepository must be created first since TradeRepository depends on it
            _instrumentRepository = new InstrumentRepository(_connectionString);
            _tradeExecutionRepository = new TradeExecutionRepository(_connectionString, _instrumentRepository);
            _openPositionRepository = new OpenPositionRepository(_connectionString);
            _historicalDataRepository = new HistoricalDataRepository(_connectionString);
        }

        public string ConnectionString => _connectionString;

        #region Instrument Operations
        public int? InsertInstrument(string conid, string symbol, string listingExchange, string currency)
        {
            return _instrumentRepository.InsertInstrument(conid, symbol, listingExchange, currency);
        }

        #endregion

        #region Trade Operations

        /// <summary>
        /// Inserts or updates trade executions from a report
        /// </summary>
        public void InsertTradeExecutions(IKBRReport report)
        {
            if (report == null || !report.Trades.Any())
            {
                Console.WriteLine("No trades found in the report.");
                return;
            }
            _instrumentRepository.UpsertInstruments(report.Trades);
            _tradeExecutionRepository.UpsertTradeExecutions(report.Trades);
        }

        /// <summary>
        /// Gets all trade executions ordered by order ID and date
        /// </summary>
        public List<TradeExecution> GetTradeExecutions()
        {
            return _tradeExecutionRepository.GetTradeExecutions();
        }

        /// <summary>
        /// Inserts or updates today's trade confirmations
        /// </summary>
        public void InsertTodayExecutions(IKBRReport report)
        {
            if (report == null || !report.TradeConfirms.Any())
            {
                Console.WriteLine("No trade confirmations found in the report.");
                return;
            }
            _instrumentRepository.UpsertInstruments(report.TradeConfirms);
            _tradeExecutionRepository.UpsertTodayExecutions(report.TradeConfirms);
        }

        #endregion

        #region OpenPosition Operations

        /// <summary>
        /// Inserts open positions from a report
        /// </summary>
        public void InsertOpenPositions(IKBRReport report)
        {
            if (report == null || string.IsNullOrEmpty(report.AccountId))
            {
                Console.WriteLine("Report or accountId is missing. Skipping Open Positions insert.");
                return;
            }

            if (!report.OpenPositions.Any())
            {
                Console.WriteLine("No open positions found in the report.");
                return;
            }

            _openPositionRepository.InsertOpenPositions(report.WhenGenerated, report.OpenPositions);
        }
        #endregion

        #region Historical Data Operations

        /// <summary>
        /// Inserts chart data for a given instrument, skipping duplicates
        /// </summary>
        public void UpsertHistoricalData(string instrumentId, List<Bar> bars)
        {
            _historicalDataRepository.UpdateHistoricalData(instrumentId, bars);
        }

        /// <summary>
        /// Gets missing date ranges for historical data for a given instrument and date range
        /// </summary>
        public List<(DateTime startDate, DateTime endDate)> GetMissingDateRanges(int instrumentId, DateTime startDate, DateTime endDate)
        {
            return _historicalDataRepository.GetMissingDateRanges(instrumentId, startDate, endDate);
        }

        #endregion

        #region Trade Summary Operations

        /// <summary>
        /// Gets aggregated trade summary for a specific order ID
        /// </summary>
        public TradeSummary? GetTradeSummaryByOrderId(long orderId)
        {
            return _tradeExecutionRepository.GetTradeSummaryByOrderId(orderId);
        }

        public int? GetInstrumentIdFromConId(string conid)
        {
            if (conid == null)
            {
                Console.WriteLine("ConId is null. Cannot retrieve InstrumentId.");
                return null;
            }
            return _instrumentRepository.GetInstrumentIdFromConId(conid);
        }

        #endregion
    }
}

