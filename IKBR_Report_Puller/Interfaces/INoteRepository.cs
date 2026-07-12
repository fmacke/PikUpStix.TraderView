using IKBR_Report_Puller.Domain;
using System.Collections.Generic;

namespace PikUpStix.TraderView.Interfaces
{
    /// <summary>
    /// Repository interface for Note-related database operations
    /// </summary>
    public interface INoteRepository
    {
        /// <summary>
        /// Gets all notes
        /// </summary>
        /// <returns>List of all notes</returns>
        List<Note> GetAll();

        /// <summary>
        /// Gets a note by its ID
        /// </summary>
        /// <param name="id">The note ID</param>
        /// <returns>The note, or null if not found</returns>
        Note? GetById(int id);

        /// <summary>
        /// Gets all notes for a specific position
        /// </summary>
        /// <param name="positionId">The position ID</param>
        /// <returns>List of notes for the specified position</returns>
        List<Note> GetByPositionId(int positionId);

        /// <summary>
        /// Gets all notes for a specific trade execution
        /// </summary>
        /// <param name="tradeExecutionId">The trade execution ID</param>
        /// <returns>List of notes for the specified trade execution</returns>
        List<Note> GetByTradeExecutionId(int tradeExecutionId);

        /// <summary>
        /// Gets all notes for a specific trade type
        /// </summary>
        /// <param name="tradeTypeId">The trade type ID</param>
        /// <returns>List of notes for the specified trade type</returns>
        List<Note> GetByTradeTypeId(int tradeTypeId);

        /// <summary>
        /// Inserts a new note into the database
        /// </summary>
        /// <param name="positionId">The position ID</param>
        /// <param name="tradeExecutionId">The trade execution ID (optional)</param>
        /// <param name="comment">The note comment</param>
        /// <param name="entryDate">The entry date</param>
        /// <param name="tradeTypeId">The trade type ID</param>
        /// <returns>The newly created note ID</returns>
        int Insert(int positionId, int? tradeExecutionId, string comment, DateTime entryDate, int tradeTypeId);

        /// <summary>
        /// Updates an existing note
        /// </summary>
        /// <param name="id">The ID of the note to update</param>
        /// <param name="positionId">The updated position ID</param>
        /// <param name="tradeExecutionId">The updated trade execution ID (optional)</param>
        /// <param name="comment">The updated comment</param>
        /// <param name="entryDate">The updated entry date</param>
        /// <param name="tradeTypeId">The updated trade type ID</param>
        /// <returns>True if update was successful, false otherwise</returns>
        bool Update(int id, int positionId, int? tradeExecutionId, string comment, DateTime entryDate, int tradeTypeId);

        /// <summary>
        /// Deletes a note by its ID
        /// </summary>
        /// <param name="id">The ID of the note to delete</param>
        /// <returns>True if deletion was successful, false otherwise</returns>
        bool Delete(int id);

        /// <summary>
        /// Gets notes within a date range
        /// </summary>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <returns>List of notes within the specified date range</returns>
        List<Note> GetByDateRange(DateTime startDate, DateTime endDate);
    }
}
