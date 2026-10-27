using Microsoft.Extensions.Configuration;
using Moq;
using PikUpStix.TraderView.Services;
using TraderView.Application.Interfaces.Services;

namespace IKBR_Report_Puller.Tests.Services
{
    [TestClass]
    public class ExcelReportServiceTests
    {
        private Mock<ITradingReportService> _mockTradingReportService;
        private Mock<ITradeHistoryReportService> _mockTradeHistoryReportService;
        private Mock<IConfiguration> _mockConfiguration;
        private ExcelReportService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockTradingReportService = new Mock<ITradingReportService>();
            _mockTradeHistoryReportService = new Mock<ITradeHistoryReportService>();
            _mockConfiguration = new Mock<IConfiguration>();

            // Setup configuration mock
            _mockConfiguration.Setup(c => c["Database:User"]).Returns("testuser");
            _mockConfiguration.Setup(c => c["Database:Password"]).Returns("testpass");
            _mockConfiguration.Setup(c => c["Database:Host"]).Returns("localhost");
            _mockConfiguration.Setup(c => c["Database:DbName"]).Returns("testdb");

            _service = new ExcelReportService(
                _mockTradingReportService.Object,
                _mockTradeHistoryReportService.Object);
        }
    }
}