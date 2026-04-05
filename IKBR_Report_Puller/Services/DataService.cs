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
        private readonly TradeRepository _tradeRepository;
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
            _tradeRepository = new TradeRepository(_connectionString, _instrumentRepository);
            _openPositionRepository = new OpenPositionRepository(_connectionString);
            _historicalDataRepository = new HistoricalDataRepository(_connectionString);
        }

        public string ConnectionString => _connectionString;

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

            _tradeRepository.UpsertTradeExecutions(report.Trades);
        }

        /// <summary>
        /// Gets all trade executions ordered by order ID and date
        /// </summary>
        public List<TradeExecution> GetTradeExecutions()
        {
            return _tradeRepository.GetTradeExecutions();
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

            _tradeRepository.UpsertTodayExecutions(report.TradeConfirms);
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

        /// <summary>
        /// Gets instrument details (securityID, listingExchange, symbol) for all open positions
        /// </summary>
        public List<(string securityID, string listingExchange, string symbol)> GetOpenPositionInstrumentNames(IKBRReport report)
        {
            if (report == null || !report.OpenPositions.Any())
            {
                return new List<(string securityID, string listingExchange, string symbol)>();
            }

            return _openPositionRepository.GetOpenPositionInstrumentNames(report.OpenPositions);
        }

        #endregion

        #region HistoricalData Operations

        /// <summary>
        /// Inserts chart data for a given instrument, skipping duplicates
        /// </summary>
        public void InsertChartData(string instrumentId, List<Bar> bars)
        {
            _historicalDataRepository.InsertChartData(instrumentId, bars);
        }

        #endregion

        #region Instrument Operations

        /// <summary>
        /// Upserts time series instrument data
        /// </summary>
        public void UpsertTimeSeriesData(
            string instrumentName,
            string listingExchange,
            string securityIdentifier,
            string provider,
            string dataName,
            string dataSource,
            string format,
            string frequency,
            string currency,
            DateTime date,
            double openPrice,
            double closePrice,
            double lowPrice,
            double highPrice,
            double volume)
        {
            _instrumentRepository.UpsertTimeSeriesData(
                instrumentName, listingExchange, securityIdentifier, provider, dataName,
                dataSource, format, frequency, currency, date, openPrice, closePrice,
                lowPrice, highPrice, volume);
        }

        #endregion
    }
}
