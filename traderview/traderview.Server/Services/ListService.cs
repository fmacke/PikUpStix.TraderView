using IKBR_Report_Puller.Domain;
using PikUpStix.TraderView.Interfaces;

namespace traderview.Server.Services
{
    /// <summary>
    /// Service for List operations
    /// </summary>
    public class ListService : IListService
    {
        private readonly IListRepository _listRepository;
        private readonly ILogger<ListService> _logger;

        public ListService(
            IListRepository listRepository,
            ILogger<ListService> logger)
        {
            _listRepository = listRepository;
            _logger = logger;
        }

        /// <summary>
        /// Gets all list items asynchronously
        /// </summary>
        public async Task<List<ListItem>> GetAllAsync()
        {
            try
            {
                return await Task.Run(() => _listRepository.GetAll());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all list items");
                throw;
            }
        }

        /// <summary>
        /// Gets a list item by its ID asynchronously
        /// </summary>
        public async Task<ListItem?> GetByIdAsync(int id)
        {
            try
            {
                return await Task.Run(() => _listRepository.GetById(id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching list item with ID {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Gets all items for a specific list name asynchronously
        /// </summary>
        public async Task<List<ListItem>> GetByListNameAsync(string listName)
        {
            try
            {
                return await Task.Run(() => _listRepository.GetByListName(listName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching items for list {ListName}", listName);
                throw;
            }
        }

        /// <summary>
        /// Creates a new list item asynchronously
        /// </summary>
        public async Task<int> CreateAsync(string listName, string item)
        {
            try
            {
                return await Task.Run(() => _listRepository.Insert(listName, item));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating list item: {ListName} - {Item}", listName, item);
                throw;
            }
        }

        /// <summary>
        /// Updates an existing list item asynchronously
        /// </summary>
        public async Task<bool> UpdateAsync(int id, string listName, string item)
        {
            try
            {
                return await Task.Run(() => _listRepository.Update(id, listName, item));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating list item with ID {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Deletes a list item by its ID asynchronously
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                return await Task.Run(() => _listRepository.Delete(id));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting list item with ID {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Gets distinct list names asynchronously
        /// </summary>
        public async Task<List<string>> GetDistinctListNamesAsync()
        {
            try
            {
                return await Task.Run(() => _listRepository.GetDistinctListNames());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching distinct list names");
                throw;
            }
        }
    }
}
