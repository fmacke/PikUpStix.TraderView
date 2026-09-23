using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities.FMP;
using PikUpStix.TraderView.Services.MarketData;

namespace IKBR_Report_Puller.Tests.Services
{
    [TestClass]
    public class FinancialModellingPrepServiceTests
    {
        [TestMethod]
        public async Task RunScreenerAsync_ReturnsCandidatesFromCanSlimService_WhenLatestSnapshotIsToday()
        {
            // Arrange
            var mockEconomicRepo = new Mock<IEconomicCalendarRepository>();
            var mockHistoricalRepo = new Mock<IHistoricalDataRepository>();
            var mockInstrumentRepo = new Mock<IInstrumentRepository>();
            var mockCanSlimService = new Mock<ICanSlimScreenerService>();

            var snapshot = new CanSlimScreenerSnapshot { Id = 42, CreatedAt = DateTime.Today };
            var expectedCandidates = new List<CanSlimCandidate>
            {
                new CanSlimCandidate { Symbol = "AAPL" },
                new CanSlimCandidate { Symbol = "MSFT" }
            };

            mockCanSlimService.Setup(x => x.GetLatestScreenerSnapShot()).ReturnsAsync(snapshot);
            mockCanSlimService.Setup(x => x.GetAllBySnapshotIdAsync(snapshot.Id)).ReturnsAsync(expectedCandidates);

            var httpClient = new HttpClient(); // not used in this test path

            var service = new FinancialModellingPrepService(
                httpClient,
                mockEconomicRepo.Object,
                mockHistoricalRepo.Object,
                mockInstrumentRepo.Object,
                mockCanSlimService.Object,
                apiKey: "testkey",
                baseUrl: "https://fmp.test",
                outputFilePath: "./out"
            );

            // Act
            var result = await ((IMarketDataService)service).RunScreenerAsync(new CanSlimScreenerCriteria());

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedCandidates.Count, result.Count);
            Assert.AreEqual(expectedCandidates[0].Symbol, result[0].Symbol);
            Assert.AreEqual(expectedCandidates[1].Symbol, result[1].Symbol);
        }
    }
}
