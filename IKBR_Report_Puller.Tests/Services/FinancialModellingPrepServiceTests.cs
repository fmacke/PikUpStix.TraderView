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

            var service = new FinancialModellingPrepCompanyScreeningService(
                httpClient,
                mockCanSlimService.Object,
                apiKey: "testkey",
                baseUrl: "https://fmp.test"
            );

            // Act
            var result = await ((ICompanyScreeningService)service).RunScreenerAsync(new CanSlimScreenerCriteria());

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedCandidates.Count, result.Count);
            Assert.AreEqual(expectedCandidates[0].Symbol, result[0].Symbol);
            Assert.AreEqual(expectedCandidates[1].Symbol, result[1].Symbol);
        }
    }
}
