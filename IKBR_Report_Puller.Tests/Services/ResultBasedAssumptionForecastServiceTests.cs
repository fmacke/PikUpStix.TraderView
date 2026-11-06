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
            // 28 winning trades out of 60 = 46% win rate, 14% avg gain, 7% avg loss
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
            Assert.AreEqual(7000m, Math.Round(result.AverageCurrencyGainOnWinningTrade,0));
            Assert.AreEqual(28, result.NumberOfWinningTrades);
            Assert.AreEqual(-3500m, Math.Round(result.AverageCurrencyLossOnLosingTrade,0));
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
        /// Creates 60 test positions (28 winning, 32 losing) to produce the expected forecast results.
        /// 
        /// Test expectations:
        /// - NumberOfWinningTrades: 28
        /// - NumberOfLosingTrades: 32
        /// - AverageCurrencyGainOnWinningTrade: 7000m
        /// - AverageCurrencyLossOnLosingTrade: -3500m
        /// - PositionSize: 50000m (from 200000 × 0.25)
        /// 
        /// Calculations:
        /// - Total gain needed: 7000 × 28 = 196,000
        /// - Total loss needed: -3500 × 32 = -112,000
        /// - Gain per position: 196,000 / 28 = 7000m
        /// - Loss per position: -112,000 / 32 = -3500m
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

            // Create 28 winning positions with 7000m net margin each
            for (int i = 0; i < 28; i++)
            {
                positions.Add(CreatePositionWithNetMargin(
                    positionId: i + 1,
                    instrument: instrument,
                    quantity: 500m,
                    buyPrice: 100m,
                    desiredNetMargin: 7000m
                ));
            }

            // Create 32 losing positions with -3500m net margin each
            for (int i = 0; i < 33; i++)
            {
                positions.Add(CreatePositionWithNetMargin(
                    positionId: 100 + i + 1,
                    instrument: instrument,
                    quantity: 500m,
                    buyPrice: 100m,
                    desiredNetMargin: -3500m
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
