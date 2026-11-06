using PikUpStix.TraderView.Services;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Models;
using TraderView.Domain.Entities;

namespace IKBR_Report_Puller.Tests.Services
{
    [TestClass]
    public class ResultBasedAssumptionForecastServiceTests
    {
        private ITradeHistoryReportService _tradeHistoryReportService = null!;

        [TestInitialize]
        public void Setup()
        {
            _tradeHistoryReportService = new TradeHistoryReportService();
        }

        [TestMethod]
        public async Task CalculateForecast_WithValidInputs_ReturnsExpectedResults()
        {
            // Arrange - Create positions that produce the expected trade metrics
            // 46 winning trades out of 100 = 46% win rate, 14% avg gain, 7% avg loss
            var positions = CreateTestPositions();

            // Act
            var result = await _tradeHistoryReportService.CreateDesiredOutcomeForecast(
                portfolioSize: 200000m,
                positionSizePercent: 0.25m,
                desiredReturnPercent: 0.4m,
                positions: positions
            );

            // Assert - Original test expectations
            Assert.IsNotNull(result);
            Assert.AreEqual(7000m, result.AverageCurrencyGainOnWinningTrade);
            Assert.AreEqual(28, result.NumberOfWinningTrades);
            Assert.AreEqual(-3500m, result.AverageCurrencyLossOnLosingTrade);
            Assert.AreEqual(32, result.NumberOfLosingTrades);
            Assert.AreEqual(0.5m, result.GainLossRatio);
            Assert.AreEqual(50000m, result.PositionSize);
            Assert.AreEqual(0.0266m, result.ExpectedNetReturnPercent);
            Assert.AreEqual(1300m, result.ExpectedNetReturnCurrency);
            Assert.AreEqual(80000m, result.GoalCurrency);
            Assert.AreEqual(60, result.NumberOfTradesNeededToReachGoal);
            Assert.AreEqual((1.7m / 1), result.AdjustedGainLossRatio);
            Assert.AreEqual(0.19m, result.OtpimalF);
        }

        /// <summary>
        /// Creates 100 test positions (46 winning, 54 losing) to produce exactly 46% win rate.
        /// 
        /// This test mimics the original which passed parameters directly:
        /// - var inputs = new DesiredPerformanceInputs(200000m, 0.25m, 0.4m, 0.14m, 0.07m, 0.46m);
        /// 
        /// Since the new interface requires positions, we create them to produce those exact percentages:
        /// - WinRatePercentage = 46/100 = 0.46 (46% exactly)
        /// - GainPercentage = 14 → after /100 = 0.14
        /// - LossPercentage = 7 → after /100 = 0.07
        /// 
        /// With 100 positions × $50,000 = $5,000,000 total cost:
        /// - Sum winning NetMargins = 5,000,000 × 0.14 = 700,000
        ///   → Per winning position = 700,000 / 46 ≈ 15,217
        /// - Sum losing NetMargins = 5,000,000 × -0.07 = -350,000
        ///   → Per losing position = -350,000 / 54 ≈ -6,481
        /// </summary>
        private IReadOnlyList<Position> CreateTestPositions()
        {
            var positions = new List<Position>();
            var instrument = new Instrument
            {
                Id = 1,
                InstrumentName = "TEST",
                DataName = "TEST",
                Currency = "USD"
            };

            // Create 46 winning positions
            for (int i = 0; i < 46; i++)
            {
                positions.Add(CreatePositionWithNetMargin(
                    positionId: i + 1,
                    instrument: instrument,
                    quantity: 500m,
                    buyPrice: 100m,
                    desiredNetMargin: 15217m
                ));
            }

            // Create 54 losing positions
            for (int i = 0; i < 54; i++)
            {
                positions.Add(CreatePositionWithNetMargin(
                    positionId: 100 + i + 1,
                    instrument: instrument,
                    quantity: 500m,
                    buyPrice: 100m,
                    desiredNetMargin: -6481m
                ));
            }

            return positions.AsReadOnly();
        }

        /// <summary>
        /// Creates a position with a specific desired net margin.
        /// NetMargin = GrossMargin + IbCommission
        /// GrossMargin = TotalSalesValue - TotalPurchaseCost
        /// TotalPurchaseCost = AverageCostBasis * Quantity
        /// TotalSalesValue = AverageSalesValue * Quantity
        /// </summary>
        private Position CreatePositionWithNetMargin(int positionId, Instrument instrument, 
            decimal quantity, decimal buyPrice, decimal desiredNetMargin)
        {
            decimal totalPurchaseCost = quantity * buyPrice;
            decimal commission = 20m;

            // We need: NetMargin = GrossMargin + Commission
            // So: GrossMargin = NetMargin - Commission
            decimal desiredGrossMargin = desiredNetMargin - commission;

            // GrossMargin = TotalSalesValue - TotalPurchaseCost
            // So: TotalSalesValue = TotalPurchaseCost + GrossMargin
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
                MaximumFavourableExcursion = desiredGrossMargin * 0.5m,
                MaximumAdverseExcursion = 0m,
                TradeExecutions = new List<TradeExecution> { buyExecution, sellExecution }
            };

            return position;
        }
    }
}
