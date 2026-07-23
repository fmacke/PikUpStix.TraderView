using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace PikUpStix.TraderView.Data.Repositories
{
    /// <summary>
    /// Base repository class providing common database operations
    /// </summary>
    public abstract class BaseRepository
    {
        protected readonly string ConnectionString;

        protected BaseRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Executes a database operation with automatic connection management and error handling
        /// </summary>
        protected void ExecuteDatabaseOperation(Action<SqlConnection> operation)
        {
            try
            {
                using SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Open();
                operation(connection);
            }
            catch (SqlException e)
            {
                Console.WriteLine($"\nDatabase error: {e.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Executes a database operation and returns a result
        /// </summary>
        protected T ExecuteDatabaseOperation<T>(Func<SqlConnection, T> operation)
        {
            try
            {
                using SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Open();
                return operation(connection);
            }
            catch (SqlException e)
            {
                Console.WriteLine($"\nDatabase error: {e.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nAn error occurred: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Executes a parameterized SQL command within a transaction
        /// </summary>
        protected void ExecuteCommand(SqlConnection connection, SqlTransaction transaction, string query, Dictionary<string, object> parameters)
        {
            try
            {
                using SqlCommand cmd = new SqlCommand(query, connection, transaction);
                foreach (var param in parameters)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
                cmd.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                string boom = ex.Message;
            }
        }

        /// <summary>
        /// Executes a scalar query and returns a single value
        /// </summary>
        protected T ExecuteScalar<T>(SqlConnection connection, SqlTransaction transaction, string query, Dictionary<string, object> parameters = null)
        {
            using SqlCommand cmd = new SqlCommand(query, connection, transaction);
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }
            var result = cmd.ExecuteScalar();
            return result != null ? (T)result : default(T);
        }

        /// <summary>
        /// Checks if a record exists based on a query
        /// </summary>
        protected bool RecordExists(SqlConnection connection, SqlTransaction transaction, string query, Dictionary<string, object> parameters)
        {
            int count = ExecuteScalar<int>(connection, transaction, query, parameters);
            return count > 0;
        }
    }
}
