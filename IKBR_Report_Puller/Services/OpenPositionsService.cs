using TraderView.Domain.Entities;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;

namespace PikUpStix.TraderView.Services
{
    public class OpenPositionsService : IOpenPositionsService
    {
        private readonly ITradeExecutionRepository _tradeExecutionRepository;

        public OpenPositionsService(ITradeExecutionRepository tradeExecutionRepository, IPositionRepository positionRepository)
        {
            _tradeExecutionRepository = tradeExecutionRepository;
        }

        async Task<List<Position>> IOpenPositionsService.GetOpenPositionsAsync()
        {
            return await Task.Run(() => _tradeExecutionRepository.GetOpenPositions());
        }
    }
}
