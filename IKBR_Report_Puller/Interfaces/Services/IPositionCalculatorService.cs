using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services;

public interface IPositionCalculatorService
{
    Task<IReadOnlyList<PositionCalculator>> GetAllAsync();
    Task<PositionCalculator?> GetByIdAsync(int id);
    Task<PositionCalculator> CreateAsync(PositionCalculator entity);
    Task UpdateAsync(PositionCalculator entity);
    Task DeleteAsync(int id);
}
