using TraderView.Domain.Entities;
using TraderView.Application.Interfaces.Persistence;
namespace TraderView.Application.Interfaces.Repositories
{
    /// <summary>
    /// Async repository interface for List-related database operations
    /// </summary>
    public interface IListRepository : IRepository<ListItem>
    {
        Task<IReadOnlyList<ListItem>> GetByCategoryAsync(string category);

        Task<int> InsertAsync(string category, string name);

        Task<bool> UpdateAsync(int id, string category, string name, string? description, bool isActive, DateTime updatedAt);

        Task<bool> DeleteAsync(int id);

        Task<IReadOnlyList<string>> GetDistinctCategoriesAsync();
    }
}
