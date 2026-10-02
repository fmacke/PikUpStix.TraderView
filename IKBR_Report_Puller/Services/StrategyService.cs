using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities;

namespace PikUpStix.TraderView.Services
{
    public class StrategyService : IStrategyService
    {
        private readonly IStrategyRepository _strategyRepository;
        public StrategyService(IStrategyRepository strategyRepository)
        {
            _strategyRepository = strategyRepository;
        }
        public async Task<IReadOnlyList<Strategy>> GetAllAsync()
        {
            return await Task.Run(() => _strategyRepository.GetAllAsync());
        }
        public async Task<Strategy?> GetByIdAsync(int id)
        {
            return await Task.Run(() => _strategyRepository.GetByIdAsync(id));
        }
        public async Task<Strategy?> GetByListItemId(int listItemId)
        {
            return await Task.Run(() => _strategyRepository.GetByListItemId(listItemId));
        }
    }
}
