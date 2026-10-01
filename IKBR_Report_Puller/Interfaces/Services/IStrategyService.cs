using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{
    public interface IStrategyService
    {
        Task<IReadOnlyList<Strategy>> GetAllAsync();
        Task<Strategy?> GetByIdAsync(int id);
    }
}