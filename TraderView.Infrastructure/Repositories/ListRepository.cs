using Microsoft.Data.SqlClient;
using TraderView.Domain.Entities;
using TraderView.Application.Interfaces.Repositories;

namespace TraderView.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for List-related database operations
    /// </summary>
    public class ListRepository : BaseRepository, IListRepository
    {
        public ListRepository(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Gets all list items
        /// </summary>
        public List<ListItem> GetAll()
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var listItems = new List<ListItem>();
                var query = "SELECT Id, Category, Name FROM ListItems ORDER BY Category, Name";

                using var command = new SqlCommand(query, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    listItems.Add(new ListItem
                    {
                        Id = reader.GetInt32(0),
                        Category = reader.GetString(1),
                        Name = reader.GetString(2)
                    });
                }

                return listItems;
            });
        }

        /// <summary>
        /// Gets a list item by its ID
        /// </summary>
        public ListItem? GetById(int id)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var query = "SELECT Id, Category, Name, Description, IsActive, CreatedAt, UpdatedAt FROM ListItems WHERE Id = @Id";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new ListItem
                    {
                        Id = reader.GetInt32(0),
                        Category = reader.GetString(1),
                        Name = reader.GetString(2),
                        Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                        IsActive = reader.GetBoolean(4),
                        CreatedAt = reader.GetDateTime(5),
                        UpdatedAt = reader.GetDateTime(6)
                    };
                }

                return null;
            });
        }

        /// <summary>
        /// Gets all items for a specific list name
        /// </summary>
        public List<ListItem> GetByListName(string category)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var listItems = new List<ListItem>();
                var query = "SELECT Id, Name, Category, Description, IsActive, CreatedAt, UpdatedAt FROM ListItems WHERE Category = @Category and IsActive = 1 ORDER BY Name";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Category", category);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    listItems.Add(new ListItem
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Category = reader.GetString(reader.GetOrdinal("Category")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                        UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                    });
                }

                return listItems;
            });
        }

        /// <summary>
        /// Inserts a new list item into the database
        /// </summary>
        public int Insert(string listName, string item)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var query = @"
                    INSERT INTO ListItems (Category, Name) 
                    VALUES (@Category, @Name);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Category", listName);
                command.Parameters.AddWithValue("@Name", item);

                var newId = command.ExecuteScalar();
                return Convert.ToInt32(newId);
            });
        }

        /// <summary>
        /// Updates an existing list item
        /// </summary>
        public bool Update(int id, string listName, string item)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var query = @"
                    UPDATE Lists 
                    SET Category = @Category, Name = @Name 
                    WHERE Id = @Id";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@Category", listName);
                command.Parameters.AddWithValue("@Name", item);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            });
        }

        /// <summary>
        /// Deletes a list item by its ID
        /// </summary>
        public bool Delete(int id)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var query = "DELETE FROM ListItems WHERE Id = @Id";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            });
        }

        /// <summary>
        /// Gets distinct list names
        /// </summary>
        public List<string> GetDistinctListNames()
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var listNames = new List<string>();
                var query = "SELECT DISTINCT Category FROM ListItems ORDER BY Category";

                using var command = new SqlCommand(query, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    listNames.Add(reader.GetString(0));
                }

                return listNames;
            });
        }
    }
}
