using DocumentFormat.OpenXml.Drawing.Charts;
using PikUpStix.TraderView.Data.Repositories;
using PikUpStix.TraderView.Domain;
using PikUpStix.TraderView.Interfaces;

namespace PikUpStix.TraderView.Services
{
    public class TradeExecutionService : ITradeExecutionService
    {
        private readonly ITradeExecutionRepository _tradeExecutionRepository;

        public TradeExecutionService(ITradeExecutionRepository tradeExecutionRepository, IPositionRepository positionRepository)
        {
            _tradeExecutionRepository = tradeExecutionRepository;
        }

        async Task<List<Position>> ITradeExecutionService.GetOpenPositionsAsync()
        {
            return await Task.Run(() => _tradeExecutionRepository.GetOpenPositions());
        }
    }
}
