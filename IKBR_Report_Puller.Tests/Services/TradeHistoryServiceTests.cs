using System;
using System.Collections.Generic;
using IKBR_Report_Puller.Domain;
using IKBR_Report_Puller.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IKBR_Report_Puller.Tests.Services
{
    [TestClass]
    public class TradeHistoryServiceTests
    {
        private TradeHistoryService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _service = new TradeHistoryService();
        }

        [TestMethod]
        public void CreateTradeHistoryReport_NoOpenPositions_OpensNewPosition()
        {
            // Arrange
            var tradeExecutions = new List<TradeExecution>
            {
                new TradeExecution
                {
                    Symbol = "AAPL",
                    Quantity = 100,
                    AveragePrice = 150.00m,
                    IbOrderID = 1,
                    TradeDate = DateTime.Now
                }
            };

            // Act
            _service.CreateTradeHistoryReport(tradeExecutions);

            // Assert
            Assert.AreEqual(1, _service.positions.Count);
            Assert.AreEqual("AAPL", _service.positions[0].Symbol);
            Assert.AreEqual(100, _service.positions[0].Quantity);
            Assert.AreEqual(150.00m, _service.positions[0].AveragePrice);
            Assert.IsFalse(_service.positions[0].IsClosed);
        }

        [TestMethod]
        public void CreateTradeHistoryReport_WithOpenPositions_ModifiesPosition()
        {
            // Arrange
            _service.positions.Add(new Position
            {
                Symbol = "AAPL",
                Quantity = 100,
                AveragePrice = 150.00m,
                IbOrderID = 1,
                TradeDate = DateTime.Now,
                IsClosed = false
            });

            var tradeExecutions = new List<TradeExecution>
            {
                new TradeExecution
                {
                    Symbol = "AAPL",
                    Quantity = 50,
                    AveragePrice = 155.00m,
                    IbOrderID = 2,
                    TradeDate = DateTime.Now.AddDays(1)
                }
            };

            // Act
            _service.CreateTradeHistoryReport(tradeExecutions);

            // Assert
            Assert.AreEqual(1, _service.positions.Count);
            Assert.AreEqual(150, _service.positions[0].Quantity);
        }

        [TestMethod]
        public void CreateTradeHistoryReport_MultipleExecutions_ProcessesAll()
        {
            // Arrange
            var tradeExecutions = new List<TradeExecution>
            {
                new TradeExecution
                {
                    Symbol = "AAPL",
                    Quantity = 100,
                    AveragePrice = 150.00m,
                    IbOrderID = 1,
                    TradeDate = DateTime.Now
                },
                new TradeExecution
                {
                    Symbol = "MSFT",
                    Quantity = 50,
                    AveragePrice = 200.00m,
                    IbOrderID = 2,
                    TradeDate = DateTime.Now
                }
            };

            // Act
            _service.CreateTradeHistoryReport(tradeExecutions);

            // Assert
            Assert.AreEqual(2, _service.positions.Count);
        }

        [TestMethod]
        public void UpdatePosition_RevisedQuantityZero_ClosesPosition()
        {
            // Arrange
            var position = new Position
            {
                Symbol = "AAPL",
                Quantity = 100,
                AveragePrice = 150.00m,
                IbOrderID = 1,
                TradeDate = DateTime.Now,
                IsClosed = false
            };

            var tradeExecution = new TradeExecution
            {
                Symbol = "AAPL",
                Quantity = -100,
                AveragePrice = 155.00m,
                IbOrderID = 2,
                TradeDate = DateTime.Now.AddDays(1)
            };

            // Act
            _service.UpdatePosition(tradeExecution, position);

            // Assert
            Assert.IsTrue(position.IsClosed);
        }

        [TestMethod]
        public void UpdatePosition_RevisedQuantityPositiveAndIsLong_UpdatesPositionAndAveragePrice()
        {
            // Arrange
            var position = new Position
            {
                Symbol = "AAPL",
                Quantity = 100,
                AveragePrice = 150.00m,
                IbOrderID = 1,
                TradeDate = DateTime.Now,
                IsClosed = false
            };

            var tradeExecution = new TradeExecution
            {
                Symbol = "AAPL",
                Quantity = -50,
                AveragePrice = 160.00m,
                IbOrderID = 2,
                TradeDate = DateTime.Now.AddDays(1)
            };

            // Act
            _service.UpdatePosition(tradeExecution, position);

            // Assert
            Assert.AreEqual(50, position.Quantity);
            Assert.AreEqual(155.00m, position.AveragePrice);
        }

        [TestMethod]
        public void UpdatePosition_RevisedQuantityNegativeAndIsShort_UpdatesPositionAndAveragePrice()
        {
            // Arrange
            var position = new Position
            {
                Symbol = "AAPL",
                Quantity = -100,
                AveragePrice = 150.00m,
                IbOrderID = 1,
                TradeDate = DateTime.Now,
                IsClosed = false
            };

            var tradeExecution = new TradeExecution
            {
                Symbol = "AAPL",
                Quantity = 50,
                AveragePrice = 160.00m,
                IbOrderID = 2,
                TradeDate = DateTime.Now.AddDays(1)
            };

            // Act
            _service.UpdatePosition(tradeExecution, position);

            // Assert
            Assert.AreEqual(-50, position.Quantity);
            Assert.AreEqual(155.00m, position.AveragePrice);
        }

        [TestMethod]
        public void UpdatePosition_RevisedQuantityPositiveAndIsShort_ReversesPosition()
        {
            // Arrange
            _service.positions.Clear();
            var position = new Position
            {
                Symbol = "AAPL",
                Quantity = -100,
                AveragePrice = 150.00m,
                IbOrderID = 1,
                TradeDate = DateTime.Now,
                IsClosed = false
            };

            var tradeExecution = new TradeExecution
            {
                Symbol = "AAPL",
                Quantity = 150,
                AveragePrice = 160.00m,
                IbOrderID = 2,
                TradeDate = DateTime.Now.AddDays(1)
            };

            // Act
            _service.UpdatePosition(tradeExecution, position);

            // Assert
            Assert.IsTrue(position.IsClosed);
            Assert.AreEqual(1, _service.positions.Count);
            Assert.AreEqual("AAPL", _service.positions[0].Symbol);
            Assert.AreEqual(50, _service.positions[0].Quantity);
            Assert.AreEqual(160.00m, _service.positions[0].AveragePrice);
            Assert.IsFalse(_service.positions[0].IsClosed);
            Assert.AreEqual(2, _service.positions[0].IbOrderID);
        }

        [TestMethod]
        public void UpdatePosition_RevisedQuantityNegativeAndIsLong_ReversesPosition()
        {
            // Arrange
            _service.positions.Clear();
            var position = new Position
            {
                Symbol = "AAPL",
                Quantity = 100,
                AveragePrice = 150.00m,
                IbOrderID = 1,
                TradeDate = DateTime.Now,
                IsClosed = false
            };

            var tradeExecution = new TradeExecution
            {
                Symbol = "AAPL",
                Quantity = -150,
                AveragePrice = 160.00m,
                IbOrderID = 2,
                TradeDate = DateTime.Now.AddDays(1)
            };

            // Act
            _service.UpdatePosition(tradeExecution, position);

            // Assert
            Assert.IsTrue(position.IsClosed);
            Assert.AreEqual(1, _service.positions.Count);
            Assert.AreEqual("AAPL", _service.positions[0].Symbol);
            Assert.AreEqual(-50, _service.positions[0].Quantity);
            Assert.AreEqual(160.00m, _service.positions[0].AveragePrice);
            Assert.IsFalse(_service.positions[0].IsClosed);
            Assert.AreEqual(2, _service.positions[0].IbOrderID);
        }
    }
}
