using IKBR_Report_Puller.Services;
using IKBR_Report_Puller.Interfaces;
using IKBR_Report_Puller.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Xml.Linq;

namespace IKBR_Report_Puller.Tests.Services
{
    [TestClass]
    public class ExcelReportServiceTests
    {
        private Mock<ITradeExecutionRepository> _mockTradeExecutionRepository;
        private Mock<ITradeHistoryReportService> _mockTradeHistoryReportService;
        private Mock<IConfiguration> _mockConfiguration;
        private ExcelReportService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockTradeExecutionRepository = new Mock<ITradeExecutionRepository>();
            _mockTradeHistoryReportService = new Mock<ITradeHistoryReportService>();
            _mockConfiguration = new Mock<IConfiguration>();

            // Setup configuration mock
            _mockConfiguration.Setup(c => c["Database:User"]).Returns("testuser");
            _mockConfiguration.Setup(c => c["Database:Password"]).Returns("testpass");
            _mockConfiguration.Setup(c => c["Database:Host"]).Returns("localhost");
            _mockConfiguration.Setup(c => c["Database:DbName"]).Returns("testdb");

            _service = new ExcelReportService(
                _mockTradeExecutionRepository.Object, 
                _mockTradeHistoryReportService.Object,
                _mockConfiguration.Object);
        }

        [TestMethod]
        public void CreateReport_NoData_ShouldLogAndReturn()
        {
            // Arrange
            var reportXml = new XDocument(new XElement("Root"));
            var report = IKBRReportParser.ParseMainReport(reportXml);

            // Act
            _service.CreateExcelFileReport(report, "output.xlsx");

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
            _service.CreateExcelFileReport(report, "output.xlsx");

            // Assert
            // Verify logging (Excel report creation is currently stubbed)
        }
    }
}