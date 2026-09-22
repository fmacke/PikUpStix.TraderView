using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{
    /// <summary>
    /// Service interface for List operations
    /// </summary>
    public interface IListService
    {
        Task<IReadOnlyList<ListItem>> GetAllAsync();

        Task<ListItem?> GetByIdAsync(int id);

        Task<IReadOnlyList<ListItem>> GetByListNameAsync(string listName);

        Task<int> CreateAsync(string listName, string item);

        Task<bool> UpdateAsync(int id, string listName, string item);

        Task<bool> DeleteAsync(int id);

        Task<IReadOnlyList<string>> GetDistinctListNamesAsync();
    }
}
