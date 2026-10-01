using TraderView.Domain.Entities;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;

namespace PikUpStix.TraderView.Services
{
    public class PositionCalculatorService : IPositionCalculatorService
    {
        private readonly IPositionCalculatorRepository _repository;

        public PositionCalculatorService(IPositionCalculatorRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        async Task<IReadOnlyList<PositionCalculator>> IPositionCalculatorService.GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        async Task<PositionCalculator?> IPositionCalculatorService.GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        async Task<PositionCalculator> IPositionCalculatorService.CreateAsync(PositionCalculator entity)
        {
            return await _repository.AddAsync(entity);
        }

        async Task IPositionCalculatorService.UpdateAsync(PositionCalculator entity)
        {
            await _repository.UpdateAsync(entity);
        }

        async Task IPositionCalculatorService.DeleteAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity != null)
            {
                await _repository.DeleteAsync(entity);
            }
        }
    }
}
