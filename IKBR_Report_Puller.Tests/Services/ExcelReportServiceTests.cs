using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using ClosedXML.Excel;
using IKBR_Report_Puller.Interfaces;
using IKBR_Report_Puller.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace IKBR_Report_Puller.Tests.Services
{
    [TestClass]
    public class ExcelReportServiceTests
    {
        private Mock<IDataService> _mockDataService;
        private ExcelReportService _excelReportService;
        private Mock<TradeHistoryService> _mockTradeHistoryService;

        [TestInitialize]
        public void Setup()
        {
            _mockDataService = new Mock<IDataService>();
            _mockTradeHistoryService = new Mock<TradeHistoryService>();
            _mockDataService.Setup(ds => ds.ConnectionString).Returns("FakeConnectionString");

            _excelReportService = new ExcelReportService(_mockDataService.Object, _mockTradeHistoryService.Object);
        }

        [TestMethod]
        public void CreateOpenPositionsReport_NoFlexStatement_ShouldLogAndSkip()
        {
            // Arrange
            var reportXml = new XDocument(new XElement("Root"));
            string outputFilePath = "output.xlsx";

            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);

                // Act
                _excelReportService.CreateOpenPositionsReport(reportXml, outputFilePath);

                // Assert
                var output = sw.ToString();
                Assert.IsTrue(output.Contains("No FlexStatement found in the report. Skipping Excel report creation."));
            }
        }

        [TestMethod]
        public void CreateOpenPositionsReport_NoOpenPositions_ShouldLogAndSkip()
        {
            // Arrange
            var reportXml = new XDocument(new XElement("FlexStatement"));
            string outputFilePath = "output.xlsx";

            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);

                // Act
                _excelReportService.CreateOpenPositionsReport(reportXml, outputFilePath);

                // Assert
                var output = sw.ToString();
                Assert.IsTrue(output.Contains("No open positions found in the report. Skipping Excel report creation."));
            }
        }

        [TestMethod]
        public void CreateOpenPositionsReport_ValidData_ShouldCreateExcelFile()
        {
            // Arrange
            var reportXml = new XDocument(
                new XElement("FlexStatement",
                    new XAttribute("whenGenerated", "2023-10-01"),
                    new XElement("OpenPosition",
                        new XAttribute("accountId", "12345"),
                        new XAttribute("symbol", "AAPL"),
                        new XAttribute("conid", "67890"),
                        new XAttribute("position", "10"),
                        new XAttribute("costBasisPrice", "150"),
                        new XAttribute("positionValue", "1550"),
                        new XAttribute("fifoPnlUnrealized", "50")
                    )
                )
            );
            string outputFilePath = "output_[FILE_NAME].xlsx";

            // Act
            _excelReportService.CreateOpenPositionsReport(reportXml, outputFilePath);

            // Assert
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string expectedFilePath = Path.Combine(desktopPath, "output_2023-10-01.xlsx");
            Assert.IsTrue(File.Exists(expectedFilePath));

            // Cleanup
            if (File.Exists(expectedFilePath))
            {
                File.Delete(expectedFilePath);
            }
        }

        [TestMethod]
        public void CreateTradeHistoryWorksheet_ValidData_ShouldCreateWorksheet()
        {
            // Arrange
            var workbook = new XLWorkbook();
            string connectionString = "FakeConnectionString";

            // Mock the database interaction
            _mockDataService.Setup(ds => ds.ConnectionString).Returns(connectionString);

            // Act
            _excelReportService.CreateTradeHistoryWorksheet(workbook);

            // Assert
            var worksheet = workbook.Worksheet("Trade History");
            Assert.IsNotNull(worksheet, "Worksheet 'Trade History' should exist.");
            Assert.AreEqual("ibOrderID", worksheet.Cell(1, 1).Value, "First header should be 'ibOrderID'.");
            Assert.AreEqual("Symbol", worksheet.Cell(1, 2).Value, "Second header should be 'Symbol'.");
            Assert.AreEqual("Date Opened", worksheet.Cell(1, 3).Value, "Third header should be 'Date Opened'.");
        }
    }
}