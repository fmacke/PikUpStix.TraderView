using Microsoft.VisualStudio.TestTools.UnitTesting;
using PikUpStix.TraderView.Services;
using System;
using TraderView.Application.Services;

namespace IKBR_Report_Puller.Tests.Services
{
    [TestClass]
    public class RiskMatrixServiceTests
    {
        private RiskMatrixService _riskMatrixService = null!;

        [TestInitialize]
        public void Setup()
        {
            _riskMatrixService = new RiskMatrixService();
        }

        [TestMethod]
        public void CalculateExpectedRoi_WithValidInputs_CalculatesCorrectExpectancyAndSimpleRoi()
        {
            // Arrange (30% win rate, +4% gain, -2% loss, 10 trades)
            decimal gain = 4.00m;
            decimal loss = 2.00m;
            decimal winRate = 30.00m;
            int trades = 10;

            // Act
            var result = _riskMatrixService.CalculateExpectedRoi(gain, loss, winRate, trades);

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
        public void CalculateExpectedRoi_WithSpreadsheetValues_MatchesExpectedCompoundedRoi(
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
            var result = _riskMatrixService.CalculateExpectedRoi(gainDecimal, lossDecimal, winRateDecimal, tradesCount);

            // Assert
            Assert.AreEqual((double)expectedRoiDecimal, (double)result.CompoundedRoi, 0.02,
                $"Compounded ROI mismatch for Gain: {gain}%, Loss: {loss}%, WinRate: {winRate}%");
        }

        [TestMethod]
        public void CalculateExpectedRoi_WithZeroTrades_ThrowsArgumentException()
        {
            // Act
            Assert.Throws<ArgumentException>(() => _riskMatrixService.CalculateExpectedRoi(4.00m, 2.00m, 30.0m, 0));
        }
    }
}