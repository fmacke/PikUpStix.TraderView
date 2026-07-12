using IKBR_Report_Puller.Domain;
using System.Collections.Generic;

namespace PikUpStix.TraderView.Interfaces
{
    /// <summary>
    /// Repository interface for List-related database operations
    /// </summary>
    public interface IListRepository
    {
        /// <summary>
        /// Gets all list items
        /// </summary>
        /// <returns>List of all list items</returns>
        List<ListItem> GetAll();

        /// <summary>
        /// Gets a list item by its ID
        /// </summary>
        /// <param name="id">The list item ID</param>
        /// <returns>The list item, or null if not found</returns>
        ListItem? GetById(int id);

        /// <summary>
        /// Gets all items for a specific list name
        /// </summary>
        /// <param name="listName">The name of the list</param>
        /// <returns>List of items matching the list name</returns>
        List<ListItem> GetByListName(string listName);

        /// <summary>
        /// Inserts a new list item into the database
        /// </summary>
        /// <param name="listName">The name of the list</param>
        /// <param name="item">The item value</param>
        /// <returns>The newly created list item ID</returns>
        int Insert(string listName, string item);

        /// <summary>
        /// Updates an existing list item
        /// </summary>
        /// <param name="id">The ID of the list item to update</param>
        /// <param name="listName">The updated list name</param>
        /// <param name="item">The updated item value</param>
        /// <returns>True if update was successful, false otherwise</returns>
        bool Update(int id, string listName, string item);

        /// <summary>
        /// Deletes a list item by its ID
        /// </summary>
        /// <param name="id">The ID of the list item to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        bool Delete(int id);

        /// <summary>
        /// Gets distinct list names
        /// </summary>
        /// <returns>List of unique list names</returns>
        List<string> GetDistinctListNames();
    }
}
