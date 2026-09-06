using System;
using System.Collections.Generic;
using System.Text;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities;

namespace TraderView.Application.Services
{
    public class TradeExecutionService : ITradeExecutionService
    {
        private readonly ITradeExecutionRepository _repository;
        public TradeExecutionService(ITradeExecutionRepository repository)
        {
            _repository = repository;
        }
        Task<List<TradeExecution>> ITradeExecutionService.GetTradeExecutions()
        {
            var executions = _repository.GetTradeExecutions();
            return Task.FromResult(executions);
        }
    }
}
