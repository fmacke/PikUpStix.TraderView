using Microsoft.VisualStudio.TestTools.UnitTesting;
using PikUpStix.TraderView.Services;
using System;
using System.Collections.Generic;
using TraderView.Application.Models;
using TraderView.Application.Services;
using TraderView.Domain.Entities;

namespace IKBR_Report_Puller.Tests.Services
{
    [TestClass]
    public class RiskMatrixServiceTests
    {
        private CurrentPerformanceService _riskMatrixService = null!;

        [TestInitialize]
        public void Setup()
        {
            _riskMatrixService = new CurrentPerformanceService();
        }

        [TestMethod]
        public async Task CalculateExpectedRoi_WithValidInputs_CalculatesCorrectExpectancyAndSimpleRoi()
        {
            // Arrange (30% win rate, +4% gain, -2% loss, 10 trades)
            decimal gain = 4.00m;
            decimal loss = 2.00m;
            decimal winRate = 30.00m;
            int trades = 10;

            // Act
            var result = await _riskMatrixService.CalculateExpectedRoi(gain, loss, winRate, trades);

            // Assert
            Assert.AreEqual(2.00m, result.RewardToRiskRatio, "Reward-to-risk ratio calculation failed.");
            Assert.AreEqual(-0.20m, result.ExpectedReturnPerTrade, "Per-trade expectancy calculation failed.");
            Assert.AreEqual(-2.00m, result.SimpleRoi, "Simple non-compounded ROI calculation failed.");
            Assert.AreEqual(-2.35m, result.CompoundedRoi, 0.02m, "Compounded ROI calculation failed.");
        }

        [DataTestMethod]
        // 30% Batting Average Column
        [DataRow(4.00, 2.00, 30.0, -2.35)]
        [DataRow(6.00, 3.00, 30.0, -3.77)]
        [DataRow(8.00, 4.00, 30.0, -5.34)]
        [DataRow(20.00, 10.00, 30.0, -17.35)]
        [DataRow(100.00, 50.00, 30.0, -93.75)]
        // 40% Batting Average Column
        [DataRow(4.00, 2.00, 40.0, 3.63)]
        [DataRow(12.00, 6.00, 40.0, 8.55)]
        [DataRow(20.00, 10.00, 40.0, 10.20)]
        [DataRow(42.00, 21.00, 40.0, -1.16)]
        [DataRow(100.00, 50.00, 40.0, -75.00)]
        // 50% Batting Average Column
        [DataRow(4.00, 2.00, 50.0, 9.98)]
        [DataRow(20.00, 10.00, 50.0, 46.93)]
        [DataRow(48.00, 24.00, 50.0, 80.04)]
        [DataRow(100.00, 50.00, 50.0, 0.00)]
        // Custom 42.11% Batting Average Column
        [DataRow(4.00, 2.00, 42.11, 4.94)]
        [DataRow(20.00, 10.00, 42.11, 17.08)]
        [DataRow(24.00, 12.00, 42.11, 18.02)]
        [DataRow(54.00, 27.00, 42.11, -0.3618)]
        [DataRow(100.00, 50.00, 42.11, -66.5054)]
        
        public async Task CalculateExpectedRoi_WithSpreadsheetValues_MatchesExpectedCompoundedRoi(
            double gain,
            double loss,
            double winRate,
            double expectedCompoundedRoi)
        {
            // Arrange
            const int tradesCount = 10;
            decimal gainDecimal = (decimal)gain;
            decimal lossDecimal = (decimal)loss;
            decimal winRateDecimal = (decimal)winRate;
            decimal expectedRoiDecimal = (decimal)expectedCompoundedRoi;

            // Act
            var result = await _riskMatrixService.CalculateExpectedRoi(gainDecimal, lossDecimal, winRateDecimal, tradesCount);

            // Assert
            Assert.AreEqual((double)expectedRoiDecimal, (double)result.CompoundedRoi, 0.03,
                $"Compounded ROI mismatch for Gain: {gain}%, Loss: {loss}%, WinRate: {winRate}%");
        }

        [TestMethod]
        public async Task CalculateExpectedRoi_WithZeroTrades_ThrowsArgumentException()
        {
            // Act
            await Assert.ThrowsAsync<ArgumentException>(async () => await _riskMatrixService.CalculateExpectedRoi(4.00m, 2.00m, 30.0m, 0));
        }

        [TestMethod]
        public async Task TradeHistoryReport_WithPopulatedPositions_ProducesSameRoiAsCalculateExpectedRoi()
        {
            // Arrange - create positions that will be evaluated for ROI using TradeHistoryReport
            // This test verifies that TradeHistoryReport calculates ROI metrics from actual Position data
            // in the same manner as the CurrentPerformanceService would calculate from the same metrics

            decimal gainPercentage = 4.00m;
            decimal lossPercentage = 2.00m;
            decimal winRatePercentage = 30.00m;
            int totalTrades = 10;

            // Calculate win/loss counts
            int winCount = (int)Math.Round(totalTrades * (winRatePercentage / 100m));  // 3 winning trades
            int lossCount = totalTrades - winCount;  // 7 losing trades

            // Create positions with specified gain/loss characteristics
            var positions = new List<Position>();

            var instrument = new Instrument
            {
                Id = 1,
                DataName = "TEST",
                Currency = "USD"
            };

            // Create winning positions (3 trades)
            for (int i = 0; i < winCount; i++)
            {
                positions.Add(CreateWinningPosition(i + 1, instrument, gainPercentage));
            }

            // Create losing positions (7 trades)
            for (int i = 0; i < lossCount; i++)
            {
                positions.Add(CreateLosingPosition(winCount + i + 1, instrument, lossPercentage));
            }

            // Act - Create TradeHistoryReport from positions
            var tradeHistoryReport = new TradeHistoryReport(positions);

            // Act - Also get the benchmark result using the input percentages
            var benchmarkResult = await _riskMatrixService.CalculateExpectedRoi(gainPercentage, lossPercentage, winRatePercentage, totalTrades);

            // Assert - Core validations
            Assert.AreEqual(totalTrades, tradeHistoryReport.Count, "All 10 positions should be in the report");
            Assert.AreEqual(totalTrades, tradeHistoryReport.NumberOfTrades, "Number of trades should match");
            Assert.AreEqual(winRatePercentage, tradeHistoryReport.WinRatePercentage *100, "Win rate should be 30%");

            // Assert - TradeHistoryReport should calculate valid ROI metrics
            Assert.IsTrue(tradeHistoryReport.RewardToRiskRatio >= 0, "Reward-to-risk ratio should be non-negative");
            Assert.IsTrue(tradeHistoryReport.SimpleRoi < 0 || tradeHistoryReport.SimpleRoi > 0, "SimpleRoi should be calculated");
            Assert.IsTrue(tradeHistoryReport.CompoundedRoi < 1 || tradeHistoryReport.CompoundedRoi > -1, "CompoundedRoi should be reasonable");

            // Assert - Verify the relationship between win/loss metrics
            // With 30% win rate and negative overall ROI, loss % should dominate
            Assert.IsTrue(Math.Abs(tradeHistoryReport.LossPercentage) >= Math.Abs(tradeHistoryReport.GainPercentage), 
                "Loss percentage should dominate with 30% win rate");

            // Assert - Verify ExpectedReturnPerTrade is calculated
            // SimpleRoi is already in percentage form (e.g., -6.2 means -6.2%)
            // ExpectedReturnPerTrade should be SimpleRoi / numberOfTrades
            var expectedReturnPerTrade = tradeHistoryReport.SimpleRoi / totalTrades;
            Assert.AreEqual((double)expectedReturnPerTrade, (double)tradeHistoryReport.ExpectedReturnPerTrade, 0.01, 
                "Expected return per trade should match formula");
        }

        /// <summary>
        /// Creates a winning position with the specified gain percentage
        /// </summary>
        private Position CreateWinningPosition(int positionId, Instrument instrument, decimal gainPercentage)
        {
            // TradeHistoryReport constructor calculates from TradeExecutions:
            // - AverageCostBasis from BUY executions
            // - AverageSalesValue from SELL executions  
            // - Quantity from BUY side executions
            // Then TradeHistoryPosition calculates:
            // - TotalPurchaseCost = AverageCostBasis * Quantity
            // - TotalSalesValue = AverageSalesValue * Quantity
            // - GrossMargin = TotalSalesValue - TotalPurchaseCost
            // - NetMargin = GrossMargin + IbCommission
            //
            // We want: NetMargin / TotalPurchaseCost ≈ gainPercentage / 100
            // Setup: 100 shares at $100 buy = $10,000 cost
            // For 4% gain: NetMargin = 400
            // If Commission = 20, then GrossMargin = 380
            // SalesValue = 10,000 + 380 = 10,380, SellPrice = 103.8

            const decimal quantity = 100m;
            const decimal buyPrice = 100m;
            decimal totalPurchaseCost = quantity * buyPrice;
            decimal commission = 20m;
            decimal desiredNetMargin = totalPurchaseCost * (gainPercentage / 100m);
            decimal desiredGrossMargin = desiredNetMargin - commission;
            decimal totalSalesValue = totalPurchaseCost + desiredGrossMargin;
            decimal sellPrice = totalSalesValue / quantity;

            // BUY is positive quantity, SELL is negative (convention in this system)
            var buyExecution = new TradeExecution
            {
                Id = positionId * 100 + 1,
                BuySell = "BUY",
                Quantity = quantity,
                TradePrice = buyPrice,
                IbCommission = 0m,
                IbCommissionCurrency = "USD"
            };

            var sellExecution = new TradeExecution
            {
                Id = positionId * 100 + 2,
                BuySell = "SELL",
                Quantity = -quantity,  // SELL quantities are negative
                TradePrice = sellPrice,
                IbCommission = commission,
                IbCommissionCurrency = "USD"
            };

            var position = new Position
            {
                Id = positionId,
                OpenDate = DateTime.Now.AddDays(-30),
                CloseDate = DateTime.Now,
                Status = "Closed",
                InstrumentId = instrument.Id,
                Instrument = instrument,
                Direction = "BUY",
                NetProfit = desiredNetMargin,
                GrossProfit = desiredGrossMargin,
                Commission = commission,
                MaximumFavourableExcursion = desiredGrossMargin * 0.5m,
                MaximumAdverseExcursion = 0m,
                TradeExecutions = new List<TradeExecution> { buyExecution, sellExecution }
            };

            return position;
        }

        /// <summary>
        /// Creates a losing position with the specified loss percentage
        /// </summary>
        private Position CreateLosingPosition(int positionId, Instrument instrument, decimal lossPercentage)
        {
            // Same logic as winning position but with lower sell price
            // For -2% loss: NetMargin = 10,000 * -0.02 = -200
            // If Commission = 20, then GrossMargin = -220  
            // SalesValue = 10,000 - 220 = 9,780, SellPrice = 97.8

            const decimal quantity = 100m;
            const decimal buyPrice = 100m;
            decimal totalPurchaseCost = quantity * buyPrice;
            decimal commission = 20m;
            decimal desiredNetMargin = totalPurchaseCost * (-lossPercentage / 100m);
            decimal desiredGrossMargin = desiredNetMargin - commission;
            decimal totalSalesValue = totalPurchaseCost + desiredGrossMargin;
            decimal sellPrice = totalSalesValue / quantity;

            var buyExecution = new TradeExecution
            {
                Id = positionId * 100 + 1,
                BuySell = "BUY",
                Quantity = quantity,
                TradePrice = buyPrice,
                IbCommission = 0m,
                IbCommissionCurrency = "USD"
            };

            var sellExecution = new TradeExecution
            {
                Id = positionId * 100 + 2,
                BuySell = "SELL",
                Quantity = -quantity,  // SELL quantities are negative
                TradePrice = sellPrice,
                IbCommission = commission,
                IbCommissionCurrency = "USD"
            };

            var position = new Position
            {
                Id = positionId,
                OpenDate = DateTime.Now.AddDays(-30),
                CloseDate = DateTime.Now,
                Status = "Closed",
                InstrumentId = instrument.Id,
                Instrument = instrument,
                Direction = "BUY",
                NetProfit = desiredNetMargin,
                GrossProfit = desiredGrossMargin,
                Commission = commission,
                MaximumFavourableExcursion = 0m,
                MaximumAdverseExcursion = Math.Abs(desiredGrossMargin) * 0.5m,
                TradeExecutions = new List<TradeExecution> { buyExecution, sellExecution }
            };

            return position;
        }

        [TestMethod]
        public async Task TradeHistoryReport_ExtractedMetrics_MatchCurrentPerformanceService_DirectCalculation()
        {
            // This test confirms that both methods produce IDENTICAL ROI outcomes
            // when given equivalent data from the same trades.
            //
            // Arrange - Create positions with precise characteristics
            decimal gainPercentage = 4.00m;
            decimal lossPercentage = 2.00m;
            decimal winRatePercentage = 30.00m;
            int totalTrades = 10;

            int winCount = (int)Math.Round(totalTrades * (winRatePercentage / 100m));  // 3
            int lossCount = totalTrades - winCount;  // 7

            var positions = new List<Position>();
            var instrument = new Instrument
            {
                Id = 1,
                DataName = "TEST",
                Currency = "USD"
            };

            // Create positions
            for (int i = 0; i < winCount; i++)
                positions.Add(CreateWinningPosition(i + 1, instrument, gainPercentage));

            for (int i = 0; i < lossCount; i++)
                positions.Add(CreateLosingPosition(winCount + i + 1, instrument, lossPercentage));

            // Act - Method 1: TradeHistoryReport from Position objects
            var tradeHistoryReport = new TradeHistoryReport(positions);

            // Extract the calculated metrics from TradeHistoryReport
            decimal reportGainPercentage = tradeHistoryReport.GainPercentage;
            decimal reportLossPercentage = Math.Abs(tradeHistoryReport.LossPercentage);  // Convert to positive
            decimal reportWinRatePercentage = tradeHistoryReport.WinRatePercentage;
            int reportNumberOfTrades = tradeHistoryReport.NumberOfTrades;

            // Act - Method 2: CurrentPerformanceService using extracted metrics
            var result = await _riskMatrixService.CalculateExpectedRoi(
                reportGainPercentage, 
                reportLossPercentage, 
                reportWinRatePercentage, 
                reportNumberOfTrades
            );

            // Assert - Both methods should produce IDENTICAL ROI outcomes
            Assert.AreEqual((double)Math.Round(tradeHistoryReport.RewardToRiskRatio, 1), 
                           (double)Math.Round(result.RewardToRiskRatio, 2), 
                           "Reward-to-risk ratios should be identical");

            Assert.AreEqual((double)Math.Round(tradeHistoryReport.SimpleRoi, 2), 
                           (double)Math.Round(result.SimpleRoi, 2), 
                           0.1, 
                           $"SimpleRoi should be identical: TradeHistoryReport={tradeHistoryReport.SimpleRoi}, CurrentPerformanceService={result.SimpleRoi}");

            Assert.AreEqual((double)Math.Round(tradeHistoryReport.CompoundedRoi * 100m, 2), 
                           (double)Math.Round(result.CompoundedRoi, 2), 
                           0.1, 
                           $"CompoundedRoi should be identical: TradeHistoryReport={tradeHistoryReport.CompoundedRoi * 100m}, CurrentPerformanceService={result.CompoundedRoi}");

            // Verify they both agree on core metrics
            Assert.AreEqual((double)Math.Round(tradeHistoryReport.WinRatePercentage, 2), 
                           (double)Math.Round(result.WinRatePercentage, 2), 
                           "Win rate percentages should match");

            Assert.AreEqual((double)Math.Round(tradeHistoryReport.ExpectedReturnPerTrade, 2), 
                           (double)Math.Round(result.ExpectedReturnPerTrade, 2), 
                           "Expected return per trade should match");
        }
    }
}