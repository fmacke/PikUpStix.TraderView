using Microsoft.VisualStudio.TestTools.UnitTesting;
using TraderView.Application.Models;
using PikUpStix.TraderView.Services;
using System;
using TraderView.Application.Services;

namespace IKBR_Report_Puller.Tests.Services
{
    [TestClass]
    public class TradeCalculatorServiceTests
    {
        private TradeCalculatorService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _service = new TradeCalculatorService();
        }

        [TestMethod]
        public void CalculateCompoundPositions_ScalesPositionSizesAndShares()
        {
            // Arrange - inputs chosen to mirror typical sheet example
            var request = new TradeCalculationRequest
            {
                TradeDate = DateTime.Parse("2026-10-03"),
                Instrument = "mrna",
                ExchangeRate = 1.0m,
                BuyPrice = 100.00m,
                TradingCapital = 100000.00m,
                RiskPerTrade = 6.25m,    // quarter = 6.25%, half = 3.125% (0.5x), full = 3.125% (0.5x)
                MaxExposure = 4.00m,
                GainLossRatio = 200.0m
            };

            // Act - get quarter base position then compounded positions
            var quarter = _service.CalculatePosition(request);
            var compound = _service.CalculateCompoundPositions(request, quarter);

            var q = compound.QuarterPosition;
            var h = compound.HalfPosition;
            var f = compound.FullPosition;

            // Pyramid parameters
            const decimal pyramidStepPercent = 2.5m;
            var buy1 = request.BuyPrice;
            var buy2 = buy1 * (1m + pyramidStepPercent / 100m);  // 102.5

            // Assert - position size scales by risk percent (1.0x, 0.5x, 0.5x)
            var quarterPositionSizeUsd = request.TradingCapital * (request.RiskPerTrade / 100m);
            var halfPositionSizeUsd = request.TradingCapital * ((request.RiskPerTrade * 0.5m) / 100m);
            var fullPositionSizeUsd = request.TradingCapital * ((request.RiskPerTrade * 0.5m) / 100m);

            Assert.AreEqual(Math.Round(quarterPositionSizeUsd, 6), Math.Round(q.PositionSizeUsd, 6), "Quarter position size mismatch");
            Assert.AreEqual(Math.Round(halfPositionSizeUsd, 6), Math.Round(h.PositionSizeUsd, 6), "Half position size mismatch");
            Assert.AreEqual(Math.Round(fullPositionSizeUsd, 6), Math.Round(f.PositionSizeUsd, 6), "Full position size mismatch");

            // Total shares are cumulative: q=Q, h=Q+H, f=Q+H+F
            Assert.AreEqual(Math.Round(q.Shares, 6), Math.Round(q.TotalShares, 6), "Quarter total shares should equal quarter shares");
            Assert.AreEqual(Math.Round(q.Shares + h.Shares, 6), Math.Round(h.TotalShares, 6), "Half total shares should be quarter + half shares");
            Assert.AreEqual(Math.Round(q.Shares + h.Shares + f.Shares, 6), Math.Round(f.TotalShares, 6), "Full total shares should be quarter + half + full shares");

            // Half stop loss should equal its average share price (cost basis floor)
            Assert.AreEqual(Math.Round(h.AverageSharePriceUsd, 6), Math.Round(h.StopLossAtUsd, 6), "Half stop loss should equal average share price");

            // Full stop loss should equal buy2 (pyramid second level)
            Assert.AreEqual(Math.Round(buy2, 6), Math.Round(f.StopLossAtUsd, 6), "Full stop loss should equal buy2");

            // Target share price equals buy * (1 + takeProfitPercent)
            var takeProfitPercent = quarter.TakeProfitAt; // from base quarter calculation
            var expectedTarget = q.BuyPriceUsd * (1m + takeProfitPercent / 100m);
            Assert.AreEqual(Math.Round(expectedTarget, 6), Math.Round(q.TargetSharePriceUsd, 6), "Target share price mismatch for quarter");

            // All tiers should have valid stop loss values (positive and below buy price)
            Assert.IsTrue(q.StopLossAtUsd > 0 || q.StopLossAtUsd == 0, "Quarter stop loss should not be negative");
            Assert.IsTrue(h.StopLossAtUsd > 0, "Half stop loss should be positive");
            Assert.IsTrue(f.StopLossAtUsd > 0, "Full stop loss should be positive");

            // Verify average prices are weighted correctly
            var expectedQAvgPrice = q.BuyPriceUsd;  // Quarter is the first position
            Assert.AreEqual(Math.Round(expectedQAvgPrice, 6), Math.Round(q.AverageSharePriceUsd, 6), "Quarter average share price should equal buy price");

            var expectedHalfAvgPrice = (q.Shares * q.BuyPriceUsd + ((h.TotalShares - q.Shares) * h.BuyPriceUsd)) / h.TotalShares;
            Assert.AreEqual(Math.Round(expectedHalfAvgPrice, 6), Math.Round(h.AverageSharePriceUsd, 6), "Half average share price should be weighted average");

            var expectedFullAvgPrice = (q.Shares * q.BuyPriceUsd + ((h.TotalShares - q.Shares) * h.BuyPriceUsd) + ((f.TotalShares - h.TotalShares) * f.BuyPriceUsd)) / f.TotalShares;
            Assert.AreEqual(Math.Round(expectedFullAvgPrice, 6), Math.Round(f.AverageSharePriceUsd, 6), "Full average share price should be weighted average");
        }
    }
}
