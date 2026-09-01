using TraderView.Application.Models.ResultBasedForecasting;
using TraderView.Application.Services;

namespace IKBR_Report_Puller.Tests.Services
{
    [TestClass]
    public class ResultBasedAssumptionForecastServiceTests
    {
        private ResultsBasedAssumptionForecastService _forecastService = null!;

        [TestInitialize]
        public void Setup()
        {
            _forecastService = new ResultsBasedAssumptionForecastService();
        }
        [TestMethod]
        public void CalculateForecast_WithValidInputs_ReturnsExpectedResults()
        {
            // Arrange
            var inputs = new ResultBasedAssumptionForecastInputs(200000m, 0.25m, 0.4m, 0.14m, 0.07m, 0.46m);
            // Act
            var result = _forecastService.CalculateForecast(inputs);
            // Assert
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
    }
}