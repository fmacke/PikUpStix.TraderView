using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TraderView.Infrastructure.Repositories;
using TraderView.Domain.Entities;
using TraderView.Application.Specifications.Notes;

namespace IKBR_Report_Puller.Tests.Specifications
{
    [TestClass]
    public class NoteSpecificationsTests
    {
        [TestMethod]
        public void NoteByIdSpecification_MatchesCorrectId()
        {
            var spec = new NoteByIdSpecification(5);
            Assert.IsNotNull(spec.Criteria);
            var func = spec.Criteria!.Compile();
            Assert.IsTrue(func(new Note { Id = 5 }));
            Assert.IsFalse(func(new Note { Id = 4 }));
        }

        [TestMethod]
        public void NoteByPositionSpecification_MatchesPosition()
        {
            var spec = new NoteByPositionSpecification(10);
            Assert.IsNotNull(spec.Criteria);
            var func = spec.Criteria!.Compile();
            Assert.IsTrue(func(new Note { PositionId = 10 }));
            Assert.IsFalse(func(new Note { PositionId = 9 }));
        }

        [TestMethod]
        public void NoteByTradeExecutionSpecification_MatchesExecution()
        {
            var spec = new NoteByTradeExecutionSpecification(7);
            Assert.IsNotNull(spec.Criteria);
            var func = spec.Criteria!.Compile();
            Assert.IsTrue(func(new Note { TradeExecutionId = 7 }));
            Assert.IsFalse(func(new Note { TradeExecutionId = 8 }));
        }

        [TestMethod]
        public void NoteByTradeTypeSpecification_MatchesTradeType()
        {
            var spec = new NoteByTradeTypeSpecification(3);
            Assert.IsNotNull(spec.Criteria);
            var func = spec.Criteria!.Compile();
            Assert.IsTrue(func(new Note { TradeTypeId = 3 }));
            Assert.IsFalse(func(new Note { TradeTypeId = 4 }));
        }
    }
}
