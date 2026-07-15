using IKBR_Report_Puller.Domain;

namespace PikUpStix.TraderView.Interfaces
{
    /// <summary>
    /// Service interface for List operations
    /// </summary>
    public interface IListService
    {
        /// <summary>
        /// Gets all list items asynchronously
        /// </summary>
        /// <returns>List of all list items</returns>
        Task<List<ListItem>> GetAllAsync();

        /// <summary>
        /// Gets a list item by its ID asynchronously
        /// </summary>
        /// <param name="id">The list item ID</param>
        /// <returns>The list item, or null if not found</returns>
        Task<ListItem?> GetByIdAsync(int id);

        /// <summary>
        /// Gets all items for a specific list name asynchronously
        /// </summary>
        /// <param name="listName">The name of the list</param>
        /// <returns>List of items matching the list name</returns>
        Task<List<ListItem>> GetByListNameAsync(string listName);

        /// <summary>
        /// Creates a new list item asynchronously
        /// </summary>
        /// <param name="listName">The name of the list</param>
        /// <param name="item">The item value</param>
        /// <returns>The newly created list item ID</returns>
        Task<int> CreateAsync(string listName, string item);

        /// <summary>
        /// Updates an existing list item asynchronously
        /// </summary>
        /// <param name="id">The ID of the list item to update</param>
        /// <param name="listName">The updated list name</param>
        /// <param name="item">The updated item value</param>
        /// <returns>True if update was successful, false otherwise</returns>
        Task<bool> UpdateAsync(int id, string listName, string item);

        /// <summary>
        /// Deletes a list item by its ID asynchronously
        /// </summary>
        /// <param name="id">The ID of the list item to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Gets distinct list names asynchronously
        /// </summary>
        /// <returns>List of unique list names</returns>
        Task<List<string>> GetDistinctListNamesAsync();
    }
}
