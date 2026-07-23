using Microsoft.Extensions.Configuration;
using Moq;
using PikUpStix.TraderView.Interfaces;
using PikUpStix.TraderView.Services;

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
                _mockTradeHistoryReportService.Object);
        }
    }
}