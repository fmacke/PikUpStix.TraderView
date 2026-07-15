using IKBR_Report_Puller.Domain;
using PikUpStix.TraderView.Interfaces;

namespace IKBR_Report_Puller.Services
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

        /// <summary>
        /// Gets all list items asynchronously
        /// </summary>
        public async Task<List<ListItem>> GetAllAsync()
        {
            return await Task.Run(() => _listRepository.GetAll());
        }

        /// <summary>
        /// Gets a list item by its ID asynchronously
        /// </summary>
        public async Task<ListItem?> GetByIdAsync(int id)
        {
            return await Task.Run(() => _listRepository.GetById(id));
        }

        /// <summary>
        /// Gets all items for a specific list name asynchronously
        /// </summary>
        public async Task<List<ListItem>> GetByListNameAsync(string listName)
        {
            return await Task.Run(() => _listRepository.GetByListName(listName));
        }

        /// <summary>
        /// Creates a new list item asynchronously
        /// </summary>
        public async Task<int> CreateAsync(string listName, string item)
        {
            return await Task.Run(() => _listRepository.Insert(listName, item));
        }

        /// <summary>
        /// Updates an existing list item asynchronously
        /// </summary>
        public async Task<bool> UpdateAsync(int id, string listName, string item)
        {
            return await Task.Run(() => _listRepository.Update(id, listName, item));
        }

        /// <summary>
        /// Deletes a list item by its ID asynchronously
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            return await Task.Run(() => _listRepository.Delete(id));
        }

        /// <summary>
        /// Gets distinct list names asynchronously
        /// </summary>
        public async Task<List<string>> GetDistinctListNamesAsync()
        {
            return await Task.Run(() => _listRepository.GetDistinctListNames());
        }
    }
}
