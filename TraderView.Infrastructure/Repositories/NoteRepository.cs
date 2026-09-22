using Microsoft.Data.SqlClient;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Domain.Entities;

namespace TraderView.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for Note-related database operations
    /// </summary>
    public class NoteRepository : GenericRepository<Note>, INoteRepository
    {
        public NoteRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        /// <summary>
        /// Maps a SqlDataReader to a Note entity
        /// </summary>
        protected override Note MapReaderToEntity(SqlDataReader reader)
        {
            return new Note
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                PositionId = reader.GetInt32(reader.GetOrdinal("PositionId")),
                TradeExecutionId = reader.IsDBNull(reader.GetOrdinal("TradeExecutionId")) ? null : reader.GetInt32(reader.GetOrdinal("TradeExecutionId")),
                Comment = reader.GetString(reader.GetOrdinal("Comment")),
                EntryDate = reader.GetDateTime(reader.GetOrdinal("EntryDate")),
                TradeTypeId = reader.IsDBNull(reader.GetOrdinal("TradeTypeId")) ? null : reader.GetInt32(reader.GetOrdinal("TradeTypeId")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? DateTime.Now : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                ErrorTypeId = reader.IsDBNull(reader.GetOrdinal("ErrorTypeId")) ? null : reader.GetInt32(reader.GetOrdinal("ErrorTypeId"))
            };
        }

        /// <summary>
        /// Gets all notes
        /// </summary>
        public async Task<IReadOnlyList<Note>> GetAllAsync()
        {
            return await base.GetAllAsync();
        }

        /// <summary>
        /// Gets a note by its ID
        /// </summary>
        public async Task<Note?> GetByIdAsync(int id)
        {
            var specification = new NoteByIdSpecification(id);
            return await GetSingleAsync(specification);
        }

        /// <summary>
        /// Gets all notes for a specific position
        /// </summary>
        public async Task<IReadOnlyList<Note>> GetByPositionIdAsync(int positionId)
        {
            var specification = new NoteByPositionSpecification(positionId);
            return await GetAsync(specification);
        }

        /// <summary>
        /// Gets all notes for a specific trade execution
        /// </summary>
        public async Task<IReadOnlyList<Note>> GetByTradeExecutionIdAsync(int tradeExecutionId)
        {
            var specification = new NoteByTradeExecutionSpecification(tradeExecutionId);
            return await GetAsync(specification);
        }

        /// <summary>
        /// Gets all notes for a specific trade type
        /// </summary>
        public async Task<IReadOnlyList<Note>> GetByTradeTypeIdAsync(int tradeTypeId)
        {
            var specification = new NoteByTradeTypeSpecification(tradeTypeId);
            return await GetAsync(specification);
        }

        /// <summary>
        /// Gets notes within a date range
        /// </summary>
        public async Task<IReadOnlyList<Note>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var specification = new NoteByDateRangeSpecification(startDate, endDate);
            return await GetAsync(specification);
        }

        /// <summary>
        /// Inserts a new note entity
        /// </summary>
        public async Task<Note> AddAsync(Note entity)
        {
            return await Task.Run(() =>
            {
                return ExecuteDatabaseOperation(connection =>
                {
                    var query = @"
                    INSERT INTO Notes (PositionId, TradeExecutionId, ErrorTypeId, Comment, EntryDate, UpdatedAt, TradeTypeId) 
                    VALUES (@PositionId, @TradeExecutionId, @ErrorTypeId, @Comment, @EntryDate, @UpdatedAt, @TradeTypeId);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                    using var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@PositionId", entity.PositionId);
                    command.Parameters.AddWithValue("@TradeExecutionId", entity.TradeExecutionId.HasValue ? (object)entity.TradeExecutionId.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@Comment", entity.Comment ?? string.Empty);
                    command.Parameters.AddWithValue("@EntryDate", entity.EntryDate);
                    command.Parameters.AddWithValue("@UpdatedAt", entity.UpdatedAt == default ? entity.EntryDate : entity.UpdatedAt);
                    command.Parameters.AddWithValue("@TradeTypeId", entity.TradeTypeId.HasValue ? (object)entity.TradeTypeId.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@ErrorTypeId", entity.ErrorTypeId.HasValue ? (object)entity.ErrorTypeId.Value : DBNull.Value);

                    var newId = command.ExecuteScalar();
                    entity.Id = Convert.ToInt32(newId);
                    return entity;
                });
            });
        }

        /// <summary>
        /// Inserts a new note into the database
        /// </summary>
        public async Task<int> InsertAsync(int positionId, int? tradeExecutionId, string comment, DateTime entryDate, int? tradeTypeId, int? errorTypeId)
        {
            var note = new Note
            {
                PositionId = positionId,
                TradeExecutionId = tradeExecutionId,
                Comment = comment,
                EntryDate = entryDate,
                UpdatedAt = entryDate,
                TradeTypeId = tradeTypeId,
                ErrorTypeId = errorTypeId
            };

            var inserted = await AddAsync(note);
            return inserted.Id;
        }

        /// <summary>
        /// Updates an existing note
        /// </summary>
        public async Task<bool> UpdateAsync(int id, int positionId, int? tradeExecutionId, string comment, DateTime updatedAt, int? tradeTypeId, int? errorTypeId)
        {
            return await Task.Run(() =>
            {
                return ExecuteDatabaseOperation(connection =>
                {
                    var query = @"
                    UPDATE Notes 
                    SET PositionId = @PositionId, 
                        TradeExecutionId = @TradeExecutionId, 
                        ErrorTypeId = @ErrorTypeId,
                        Comment = @Comment, 
                        UpdatedAt = @UpdatedAt, 
                        TradeTypeId = @TradeTypeId 
                    WHERE Id = @Id";

                    using var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@PositionId", positionId);
                    command.Parameters.AddWithValue("@TradeExecutionId", tradeExecutionId.HasValue ? (object)tradeExecutionId.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@ErrorTypeId", errorTypeId.HasValue ? (object)errorTypeId.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@Comment", comment ?? string.Empty);
                    command.Parameters.AddWithValue("@UpdatedAt", updatedAt);
                    command.Parameters.AddWithValue("@TradeTypeId", tradeTypeId.HasValue ? (object)tradeTypeId.Value : DBNull.Value);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                });
            });
        }

        /// <summary>
        /// Deletes a note by its ID
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            return await Task.Run(() =>
            {
                return ExecuteDatabaseOperation(connection =>
                {
                    var query = "DELETE FROM Notes WHERE Id = @Id";

                    using var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Id", id);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                });
            });
        }
    }
}
