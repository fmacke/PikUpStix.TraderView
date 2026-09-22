using System.Data;
using Microsoft.Data.SqlClient;
using TraderView.Domain.Entities;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Application.Specifications;
using TraderView.Application.Specifications.List;

namespace TraderView.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for ListItem using GenericRepository and specifications
    /// </summary>
    public class ListRepository : GenericRepository<ListItem>, IListRepository
    {
        public ListRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory)
        {
        }

        /// <summary>
        /// Map SqlDataReader to ListItem
        /// </summary>
        protected override ListItem MapReaderToEntity(SqlDataReader reader)
        {
            return new ListItem
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Category = reader.IsDBNull(reader.GetOrdinal("Category")) ? null : reader.GetString(reader.GetOrdinal("Category")),
                Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? string.Empty : reader.GetString(reader.GetOrdinal("Name")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                IsActive = reader.IsDBNull(reader.GetOrdinal("IsActive")) ? true : reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }

        public async Task<IReadOnlyList<ListItem>> GetAllAsync()
        {
            return await base.GetAllAsync();
        }

        public async Task<ListItem?> GetByIdAsync(int id)
        {
            var spec = new ListItemByIdSpecification(id);
            return await GetSingleAsync(spec);
        }

        public async Task<IReadOnlyList<ListItem>> GetByCategoryAsync(string category)
        {
            var spec = new ListItemByCategorySpecification(category);
            return await GetAsync(spec);
        }

        public async Task<int> InsertAsync(string category, string name)
        {
            var entity = new ListItem
            {
                Category = category,
                Name = name,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return (await AddAsync(entity)).Id;
        }

        public async Task<bool> UpdateAsync(int id, string category, string name, string? description, bool isActive, DateTime updatedAt)
        {
            return await Task.Run(() =>
            {
                return ExecuteDatabaseOperation(connection =>
                {
                    var query = @"
                        UPDATE ListItems
                        SET Category = @Category,
                            Name = @Name,
                            Description = @Description,
                            IsActive = @IsActive,
                            UpdatedAt = @UpdatedAt
                        WHERE Id = @Id";

                    using var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@Category", category ?? string.Empty);
                    command.Parameters.AddWithValue("@Name", name ?? string.Empty);
                    command.Parameters.AddWithValue("@Description", description ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@IsActive", isActive);
                    command.Parameters.AddWithValue("@UpdatedAt", updatedAt);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                });
            });
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await Task.Run(() =>
            {
                return ExecuteDatabaseOperation(connection =>
                {
                    var query = "DELETE FROM ListItems WHERE Id = @Id";

                    using var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Id", id);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                });
            });
        }

        public async Task<IReadOnlyList<string>> GetDistinctCategoriesAsync()
        {
            return await Task.Run(() =>
            {
                return ExecuteDatabaseOperation(connection =>
                {
                    var listNames = new List<string>();
                    var query = "SELECT DISTINCT Category FROM ListItems ORDER BY Category";

                    using var command = new SqlCommand(query, connection);
                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        listNames.Add(reader.IsDBNull(0) ? string.Empty : reader.GetString(0));
                    }

                    return (IReadOnlyList<string>)listNames;
                });
            });
        }

        // Note: provide an entity-specific AddAsync hiding the base implementation (GenericRepository's AddAsync throws NotImplementedException)
        public async Task<ListItem> AddAsync(ListItem entity)
        {
            return await Task.Run(() =>
            {
                return ExecuteDatabaseOperation(connection =>
                {
                    var query = @"
                        INSERT INTO ListItems (Category, Name, Description, IsActive, CreatedAt, UpdatedAt)
                        VALUES (@Category, @Name, @Description, @IsActive, @CreatedAt, @UpdatedAt);
                        SELECT CAST(SCOPE_IDENTITY() as int);";

                    using var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Category", entity.Category ?? string.Empty);
                    command.Parameters.AddWithValue("@Name", entity.Name ?? string.Empty);
                    command.Parameters.AddWithValue("@Description", entity.Description ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@IsActive", entity.IsActive);
                    command.Parameters.AddWithValue("@CreatedAt", entity.CreatedAt == default ? DateTime.UtcNow : entity.CreatedAt);
                    command.Parameters.AddWithValue("@UpdatedAt", entity.UpdatedAt == default ? DateTime.UtcNow : entity.UpdatedAt);

                    var newId = command.ExecuteScalar();
                    entity.Id = Convert.ToInt32(newId);
                    return entity;
                });
            });
        }
    }
}
