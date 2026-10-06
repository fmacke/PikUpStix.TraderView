using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services;

public interface IPositionCalculatorService
{
    Task<IReadOnlyList<PositionCalculator>> GetAllAsync();
    Task<PositionCalculator?> GetByIdAsync(int id);
    Task<PositionCalculator> CreateAsync(PositionCalculator entity);
    Task UpdateAsync(PositionCalculator entity);
    Task DeleteAsync(int id);
    Task<IReadOnlyList<PositionCalculator>> GetByPositionIdAsync(int positionId);
    Task<IReadOnlyList<PositionCalculator>> GetCandidatesForPositionAsync(int positionId);
    Task LinkCalculatorsAsync(int positionId, IEnumerable<int> calculatorIds);
    Task UnlinkCalculatorsAsync(IEnumerable<int> calculatorIds);
}
