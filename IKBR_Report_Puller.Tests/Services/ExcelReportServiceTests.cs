using IKBR_Report_Puller.Services;
using IKBR_Report_Puller.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Xml.Linq;

namespace IKBR_Report_Puller.Tests.Services
{
    [TestClass]
    public class ExcelReportServiceTests
    {
        private Mock<IDataService> _mockDataService;
        private Mock<ITradeHistoryReportService> _mockTradeHistoryReportService;
        private ExcelReportService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockDataService = new Mock<IDataService>();
            _mockTradeHistoryReportService = new Mock<ITradeHistoryReportService>();
            _service = new ExcelReportService(_mockDataService.Object, _mockTradeHistoryReportService.Object);
        }

        [TestMethod]
        public void CreateReport_NoFlexStatement_ShouldLogAndReturn()
        {
            // Arrange
            var reportXml = new XDocument(new XElement("Root"));

            // Act
            _service.CreateReport(reportXml, "output.xlsx");

            // Assert
            // Verify logging or other side effects
        }

        [TestMethod]
        public void CreateReport_ValidFlexStatement_ShouldCreateReport()
        {
            // Arrange
            var reportXml = new XDocument(new XElement("FlexStatement", new XAttribute("whenGenerated", "2023-10-01")));

            // Act
            _service.CreateReport(reportXml, "output.xlsx");

            // Assert
            // Verify file creation or other side effects
        }
    }
}