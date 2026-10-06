using TraderView.Domain.Entities;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Specifications.PositionCalculators;

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

        async Task<IReadOnlyList<PositionCalculator>> IPositionCalculatorService.GetByPositionIdAsync(int positionId)
        {
            var spec = new PositionCalculatorByPositionSpecification(positionId);
            return await _repository.GetAsync(spec);
        }

        async Task<IReadOnlyList<PositionCalculator>> IPositionCalculatorService.GetCandidatesForPositionAsync(int positionId)
        {
            var spec = new PositionCalculatorCandidatesSpecification(positionId);
            return await _repository.GetAsync(spec);
        }

        async Task IPositionCalculatorService.LinkCalculatorsAsync(int positionId, IEnumerable<int> calculatorIds)
        {
            if (calculatorIds == null)
                return;

            var spec = new PositionCalculatorCandidatesSpecification(positionId);
            var items = await _repository.GetAsync(spec);

            var toLink = items.Where(i => calculatorIds.Contains(i.Id));
            foreach (var item in toLink)
            {
                item.PositionId = positionId;
                await _repository.UpdateAsync(item);
            }
        }

        async Task IPositionCalculatorService.UnlinkCalculatorsAsync(IEnumerable<int> calculatorIds)
        {
            if (calculatorIds == null)
                return;

            var spec = new PositionCalculatorByIdsSpecification(calculatorIds);
            var items = await _repository.GetAsync(spec);

            foreach (var item in items)
            {
                item.PositionId = null;
                await _repository.UpdateAsync(item);
            }
        }
    }
}
