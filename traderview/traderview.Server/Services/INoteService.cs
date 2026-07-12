using IKBR_Report_Puller.Domain;

namespace traderview.Server.Services
{
    /// <summary>
    /// Service interface for Note operations
    /// </summary>
    public interface INoteService
    {
        /// <summary>
        /// Gets all notes asynchronously
        /// </summary>
        /// <returns>List of all notes</returns>
        Task<List<Note>> GetAllAsync();

        /// <summary>
        /// Gets a note by its ID asynchronously
        /// </summary>
        /// <param name="id">The note ID</param>
        /// <returns>The note, or null if not found</returns>
        Task<Note?> GetByIdAsync(int id);

        /// <summary>
        /// Gets all notes for a specific position asynchronously
        /// </summary>
        /// <param name="positionId">The position ID</param>
        /// <returns>List of notes for the specified position</returns>
        Task<List<Note>> GetByPositionIdAsync(int positionId);

        /// <summary>
        /// Gets all notes for a specific trade execution asynchronously
        /// </summary>
        /// <param name="tradeExecutionId">The trade execution ID</param>
        /// <returns>List of notes for the specified trade execution</returns>
        Task<List<Note>> GetByTradeExecutionIdAsync(int tradeExecutionId);

        /// <summary>
        /// Gets all notes for a specific trade type asynchronously
        /// </summary>
        /// <param name="tradeTypeId">The trade type ID</param>
        /// <returns>List of notes for the specified trade type</returns>
        Task<List<Note>> GetByTradeTypeIdAsync(int tradeTypeId);

        /// <summary>
        /// Creates a new note asynchronously
        /// </summary>
        /// <param name="positionId">The position ID</param>
        /// <param name="tradeExecutionId">The trade execution ID (optional)</param>
        /// <param name="comment">The note comment</param>
        /// <param name="entryDate">The entry date</param>
        /// <param name="tradeTypeId">The trade type ID</param>
        /// <returns>The newly created note ID</returns>
        Task<int> CreateAsync(int positionId, int? tradeExecutionId, string comment, DateTime entryDate, int tradeTypeId);

        /// <summary>
        /// Updates an existing note asynchronously
        /// </summary>
        /// <param name="id">The ID of the note to update</param>
        /// <param name="positionId">The updated position ID</param>
        /// <param name="tradeExecutionId">The updated trade execution ID (optional)</param>
        /// <param name="comment">The updated comment</param>
        /// <param name="entryDate">The updated entry date</param>
        /// <param name="tradeTypeId">The updated trade type ID</param>
        /// <returns>True if update was successful, false otherwise</returns>
        Task<bool> UpdateAsync(int id, int positionId, int? tradeExecutionId, string comment, DateTime entryDate, int tradeTypeId);

        /// <summary>
        /// Deletes a note by its ID asynchronously
        /// </summary>
        /// <param name="id">The ID of the note to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Gets notes within a date range asynchronously
        /// </summary>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <returns>List of notes within the specified date range</returns>
        Task<List<Note>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
