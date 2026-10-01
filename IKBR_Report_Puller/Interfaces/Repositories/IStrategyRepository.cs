using TraderView.Domain.Entities;
using TraderView.Application.Interfaces.Persistence;
namespace TraderView.Application.Interfaces.Repositories
{
    /// <summary>
    /// Async repository interface for Strategy-related database operations
    /// </summary>
    public interface IStrategyRepository : IRepository<Strategy>
    {
        
    }
}
