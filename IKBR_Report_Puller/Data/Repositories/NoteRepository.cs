using System;
using System.Collections.Generic;
using IKBR_Report_Puller.Domain;
using Microsoft.Data.SqlClient;
using PikUpStix.TraderView.Interfaces;

namespace PikUpStix.TraderView.Data.Repositories
{
    /// <summary>
    /// Repository for Note-related database operations
    /// </summary>
    public class NoteRepository : BaseRepository, INoteRepository
    {
        public NoteRepository(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Gets all notes
        /// </summary>
        public List<Note> GetAll()
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var notes = new List<Note>();
                var query = "SELECT Id, PositionId, TradeExecutionId, Comment, EntryDate, TradeTypeId FROM Notes ORDER BY EntryDate DESC";

                using var command = new SqlCommand(query, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    notes.Add(MapReaderToNote(reader));
                }

                return notes;
            });
        }

        /// <summary>
        /// Gets a note by its ID
        /// </summary>
        public Note? GetById(int id)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var query = "SELECT Id, PositionId, TradeExecutionId, Comment, EntryDate, TradeTypeId FROM Notes WHERE Id = @Id";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return MapReaderToNote(reader);
                }

                return null;
            });
        }

        /// <summary>
        /// Gets all notes for a specific position
        /// </summary>
        public List<Note> GetByPositionId(int positionId)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var notes = new List<Note>();
                var query = "SELECT Id, PositionId, TradeExecutionId, Comment, EntryDate, TradeTypeId FROM Notes WHERE PositionId = @PositionId ORDER BY EntryDate DESC";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PositionId", positionId);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    notes.Add(MapReaderToNote(reader));
                }

                return notes;
            });
        }

        /// <summary>
        /// Gets all notes for a specific trade execution
        /// </summary>
        public List<Note> GetByTradeExecutionId(int tradeExecutionId)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var notes = new List<Note>();
                var query = "SELECT Id, PositionId, TradeExecutionId, Comment, EntryDate, TradeTypeId FROM Notes WHERE TradeExecutionId = @TradeExecutionId ORDER BY EntryDate DESC";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@TradeExecutionId", tradeExecutionId);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    notes.Add(MapReaderToNote(reader));
                }

                return notes;
            });
        }

        /// <summary>
        /// Gets all notes for a specific trade type
        /// </summary>
        public List<Note> GetByTradeTypeId(int tradeTypeId)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var notes = new List<Note>();
                var query = "SELECT Id, PositionId, TradeExecutionId, Comment, EntryDate, TradeTypeId FROM Notes WHERE TradeTypeId = @TradeTypeId ORDER BY EntryDate DESC";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@TradeTypeId", tradeTypeId);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    notes.Add(MapReaderToNote(reader));
                }

                return notes;
            });
        }

        /// <summary>
        /// Inserts a new note into the database
        /// </summary>
        public int Insert(int positionId, int? tradeExecutionId, string comment, DateTime entryDate, int tradeTypeId)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var query = @"
                    INSERT INTO Notes (PositionId, TradeExecutionId, Comment, EntryDate, TradeTypeId) 
                    VALUES (@PositionId, @TradeExecutionId, @Comment, @EntryDate, @TradeTypeId);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PositionId", positionId);
                command.Parameters.AddWithValue("@TradeExecutionId", tradeExecutionId.HasValue ? (object)tradeExecutionId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@Comment", comment);
                command.Parameters.AddWithValue("@EntryDate", entryDate);
                command.Parameters.AddWithValue("@TradeTypeId", tradeTypeId);

                var newId = command.ExecuteScalar();
                return Convert.ToInt32(newId);
            });
        }

        /// <summary>
        /// Updates an existing note
        /// </summary>
        public bool Update(int id, int positionId, int? tradeExecutionId, string comment, DateTime entryDate, int tradeTypeId)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var query = @"
                    UPDATE Notes 
                    SET PositionId = @PositionId, 
                        TradeExecutionId = @TradeExecutionId, 
                        Comment = @Comment, 
                        EntryDate = @EntryDate, 
                        TradeTypeId = @TradeTypeId 
                    WHERE Id = @Id";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@PositionId", positionId);
                command.Parameters.AddWithValue("@TradeExecutionId", tradeExecutionId.HasValue ? (object)tradeExecutionId.Value : DBNull.Value);
                command.Parameters.AddWithValue("@Comment", comment);
                command.Parameters.AddWithValue("@EntryDate", entryDate);
                command.Parameters.AddWithValue("@TradeTypeId", tradeTypeId);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            });
        }

        /// <summary>
        /// Deletes a note by its ID
        /// </summary>
        public bool Delete(int id)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var query = "DELETE FROM Notes WHERE Id = @Id";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            });
        }

        /// <summary>
        /// Gets notes within a date range
        /// </summary>
        public List<Note> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var notes = new List<Note>();
                var query = "SELECT Id, PositionId, TradeExecutionId, Comment, EntryDate, TradeTypeId FROM Notes WHERE EntryDate >= @StartDate AND EntryDate <= @EndDate ORDER BY EntryDate DESC";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@StartDate", startDate);
                command.Parameters.AddWithValue("@EndDate", endDate);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    notes.Add(MapReaderToNote(reader));
                }

                return notes;
            });
        }

        /// <summary>
        /// Maps a SqlDataReader to a Note object
        /// </summary>
        private Note MapReaderToNote(SqlDataReader reader)
        {
            return new Note
            {
                Id = reader.GetInt32(0),
                PositionId = reader.GetInt32(1),
                TradeExecutionId = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                Comment = reader.GetString(3),
                EntryDate = reader.GetDateTime(4),
                TradeTypeId = reader.GetInt32(5)
            };
        }
    }
}
