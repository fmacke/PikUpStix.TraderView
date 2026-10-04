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
                RiskPerTrade = 6.25m,    // quarter = 6.25, half = 12.5, full = 25
                MaxExposure = 4.00m,
                GainLossRatio = 200.0m
            };

            // Act - get quarter base position then compounded positions
            var quarter = _service.CalculatePosition(request);
            var compound = _service.CalculateCompoundPositions(request, quarter);

            var q = compound.QuarterPosition;
            var h = compound.HalfPosition;
            var f = compound.FullPosition;

            // Assert - position sizes scale 1x,2x,4x
            Assert.AreEqual(Math.Round(q.PositionSizeUsd * 2m, 6), Math.Round(h.PositionSizeUsd, 6), "Half position size should be double quarter");
            Assert.AreEqual(Math.Round(q.PositionSizeUsd * 4m, 6), Math.Round(f.PositionSizeUsd, 6), "Full position size should be quadruple quarter");

            // Total shares scale proportionally across tiers (quarter=1x, half=2x, full=4x)
            Assert.AreEqual(Math.Round(q.TotalShares * 2m, 6), Math.Round(h.TotalShares, 6), "Half total shares should be double quarter total shares");
            Assert.AreEqual(Math.Round(q.TotalShares * 4m, 6), Math.Round(f.TotalShares, 6), "Full total shares should be quadruple quarter total shares");

            // Stop loss level computed from buy price and max exposure
            var expectedStop = q.BuyPriceUsd * (1 - (request.MaxExposure / 100m));
            Assert.AreEqual(Math.Round(expectedStop, 6), Math.Round(q.StopLossAtUsd, 6), "Stop loss at USD mismatch for quarter");

            // Target share price equals buy * (1 + takeProfitPercent)
            var takeProfitPercent = quarter.TakeProfitAt; // from base quarter calculation
            var expectedTarget = q.BuyPriceUsd * (1 + takeProfitPercent / 100m);
            Assert.AreEqual(Math.Round(expectedTarget, 6), Math.Round(q.TargetSharePriceUsd, 6), "Target share price mismatch for quarter");

            // Account risk per tier equals positionRiskGbp / trading capital *100
            var expectedAccountRiskQ = (q.PositionRiskGbp / request.TradingCapital) * 100m;
            Assert.AreEqual(Math.Round(expectedAccountRiskQ, 6), Math.Round(q.AccountRiskPercentage, 6), "Account risk percent mismatch for quarter");

            // Cumulative total shares for full equals sum of quarter+half+full shares
            var expectedTotalSharesFull = compound.QuarterPosition.Shares + compound.HalfPosition.Shares + compound.FullPosition.Shares;
            Assert.AreEqual(Math.Round(expectedTotalSharesFull, 6), Math.Round(f.TotalShares, 6), "Total shares for full position should equal sum of tier shares");
        }
    }
}
