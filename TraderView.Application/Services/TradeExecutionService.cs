using TraderView.Domain.Entities;
using PikUpStix.TraderView.Interfaces;
using TraderView.Application.Interfaces.Repositories;

namespace PikUpStix.TraderView.Services
{
    public class TradeExecutionService : ITradeExecutionService
    {
        private readonly ITradeExecutionRepository _tradeExecutionRepository;
        private readonly IPositionRepository _positionRepository;

        public TradeExecutionService(ITradeExecutionRepository tradeExecutionRepository, IPositionRepository positionRepository)
        {
            _tradeExecutionRepository = tradeExecutionRepository;
            _positionRepository = positionRepository;
        }

        async Task<List<Position>> ITradeExecutionService.GetOpenPositionsAsync()
        {
            return await _positionRepository.GetOpenPositionsAsync();
        }
    }
}
