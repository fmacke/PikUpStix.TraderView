using IKBR_Report_Puller.Services;
using IKBR_Report_Puller.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;

namespace IKBR_Report_Puller.Tests.Services
{
    [TestClass]
    public class PositionProcessorTests
    {
        private Mock<ITimeSeriesService> _mockTimeSeriesService;
        private Mock<IDataService> _mockDataService;
        private Mock<IConfiguration> _mockConfig;
        private PositionProcessor _processor;

        [TestInitialize]
        public void Setup()
        {
            _mockTimeSeriesService = new Mock<ITimeSeriesService>();
            _mockDataService = new Mock<IDataService>();
            _mockConfig = new Mock<IConfiguration>();
            _processor = new PositionProcessor(_mockTimeSeriesService.Object, _mockDataService.Object, _mockConfig.Object);
        }

        [TestMethod]
        public async Task ProcessPositionsAsync_ValidData_ShouldProcessPositions()
        {
            // Arrange
            var positionDetails = new List<(string listingExchange, string symbol, string securityID)>
            {
                ("NYSE", "AAPL", "12345")
            };

            var mainReportXml = new XElement("Root", new XElement("OpenPosition", new XAttribute("securityID", "12345"), new XAttribute("currency", "USD")));

            _mockTimeSeriesService.Setup(s => s.GetTimeSeriesDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .ReturnsAsync("{\"chart\":{\"result\":[{\"timestamp\":[1234567890],\"indicators\":{\"quote\":[{\"close\":[150.0]}]}}]}}");

            // Act
            await _processor.ProcessPositionsAsync(positionDetails, mainReportXml);

            // Assert
            _mockTimeSeriesService.Verify(s => s.GetTimeSeriesDataAsync("AAPL", "NYSE", It.IsAny<DateTime>(), It.IsAny<DateTime>(), "1d"), Times.Once);
        }

        [TestMethod]
        public async Task ProcessPositionsAsync_NoValidData_ShouldLogAndContinue()
        {
            // Arrange
            var positionDetails = new List<(string listingExchange, string symbol, string securityID)>
            {
                ("NYSE", "AAPL", "12345")
            };

            var mainReportXml = new XElement("Root", new XElement("OpenPosition", new XAttribute("securityID", "12345"), new XAttribute("currency", "USD")));

            _mockTimeSeriesService.Setup(s => s.GetTimeSeriesDataAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .ReturnsAsync("{\"chart\":{\"result\":[{\"timestamp\":null,\"indicators\":{\"quote\":[null]}}]}}");

            // Act
            await _processor.ProcessPositionsAsync(positionDetails, mainReportXml);

            // Assert
            _mockTimeSeriesService.Verify(s => s.GetTimeSeriesDataAsync("AAPL", "NYSE", It.IsAny<DateTime>(), It.IsAny<DateTime>(), "1d"), Times.Once);
        }
    }
}