using IKBR_Report_Puller.Services;
using IKBR_Report_Puller.Interfaces;
using IKBR_Report_Puller.Domain;
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
        public void CreateReport_NoData_ShouldLogAndReturn()
        {
            // Arrange
            var reportXml = new XDocument(new XElement("Root"));
            var report = IKBRReportParser.ParseMainReport(reportXml);

            // Act
            _service.CreateReport(report, "output.xlsx");

            // Assert
            // Verify logging or other side effects
        }

        [TestMethod]
        public void CreateReport_ValidReport_ShouldLog()
        {
            // Arrange
            var reportXml = new XDocument(
                new XElement("FlexStatement", 
                    new XAttribute("whenGenerated", "20231001;120000"),
                    new XAttribute("accountId", "U1234567")));
            var report = IKBRReportParser.ParseMainReport(reportXml);

            // Act
            _service.CreateReport(report, "output.xlsx");

            // Assert
            // Verify logging (Excel report creation is currently stubbed)
        }
    }
}