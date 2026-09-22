using TraderView.Domain.Entities;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;

namespace PikUpStix.TraderView.Services
{
    /// <summary>
    /// Service for List operations
    /// </summary>
    public class ListService : IListService
    {
        private readonly IListRepository _listRepository;

        public ListService(IListRepository listRepository)
        {
            _listRepository = listRepository;
        }

        public async Task<IReadOnlyList<ListItem>> GetAllAsync()
        {
            return await _listRepository.GetAllAsync();
        }

        public async Task<ListItem?> GetByIdAsync(int id)
        {
            return await _listRepository.GetByIdAsync(id);
        }

        public async Task<IReadOnlyList<ListItem>> GetByListNameAsync(string listName)
        {
            return await _listRepository.GetByCategoryAsync(listName);
        }

        public async Task<int> CreateAsync(string listName, string item)
        {
            return await _listRepository.InsertAsync(listName, item);
        }

        public async Task<bool> UpdateAsync(int id, string listName, string item)
        {
            // Preserve existing description/isActive by fetching current entity if needed; simple update uses provided values
            return await _listRepository.UpdateAsync(id, listName, item, null, true, DateTime.UtcNow);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _listRepository.DeleteAsync(id);
        }

        public async Task<IReadOnlyList<string>> GetDistinctListNamesAsync()
        {
            return await _listRepository.GetDistinctCategoriesAsync();
        }
    }
}
