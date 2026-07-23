using System;
using System.Collections.Generic;
using IKBR_Report_Puller.Domain;
using Microsoft.Data.SqlClient;
using PikUpStix.TraderView.Interfaces;

namespace PikUpStix.TraderView.Data.Repositories
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
                var query = "SELECT Id, ListName, Item FROM Lists ORDER BY ListName, Item";

                using var command = new SqlCommand(query, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    listItems.Add(new ListItem
                    {
                        Id = reader.GetInt32(0),
                        ListName = reader.GetString(1),
                        Item = reader.GetString(2)
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
                var query = "SELECT Id, ListName, Item FROM Lists WHERE Id = @Id";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new ListItem
                    {
                        Id = reader.GetInt32(0),
                        ListName = reader.GetString(1),
                        Item = reader.GetString(2)
                    };
                }

                return null;
            });
        }

        /// <summary>
        /// Gets all items for a specific list name
        /// </summary>
        public List<ListItem> GetByListName(string listName)
        {
            return ExecuteDatabaseOperation(connection =>
            {
                var listItems = new List<ListItem>();
                var query = "SELECT Id, ListName, Item FROM Lists WHERE ListName = @ListName ORDER BY Item";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ListName", listName);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    listItems.Add(new ListItem
                    {
                        Id = reader.GetInt32(0),
                        ListName = reader.GetString(1),
                        Item = reader.GetString(2)
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
                    INSERT INTO Lists (ListName, Item) 
                    VALUES (@ListName, @Item);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ListName", listName);
                command.Parameters.AddWithValue("@Item", item);

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
                    SET ListName = @ListName, Item = @Item 
                    WHERE Id = @Id";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@ListName", listName);
                command.Parameters.AddWithValue("@Item", item);

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
                var query = "DELETE FROM Lists WHERE Id = @Id";

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
                var query = "SELECT DISTINCT ListName FROM Lists ORDER BY ListName";

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
