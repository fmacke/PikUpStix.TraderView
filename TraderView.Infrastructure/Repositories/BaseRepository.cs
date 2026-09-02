using Microsoft.Data.SqlClient;
using TraderView.Application.Features;

namespace TraderView.Infrastructure.Repositories
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
        protected void ExecuteCommand(SqlConnection connection, SqlTransaction transaction, IQueryWithParameters queryWithParameters)
        {
            try
            {
                using SqlCommand cmd = new SqlCommand(queryWithParameters.Script, connection, transaction);
                foreach (var param in queryWithParameters.Parameters)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
                cmd.ExecuteNonQuery();
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
        /// Executes a scalar query and returns a single value
        /// </summary>
        protected T ExecuteScalar<T>(SqlConnection connection, SqlTransaction transaction, IQueryWithParameters queryWithParams)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }

                using SqlCommand cmd = new SqlCommand(queryWithParams.Script, connection, transaction);

                if (queryWithParams.Parameters != null)
                {
                    foreach (var param in queryWithParams.Parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }

                var result = cmd.ExecuteScalar();

                // Handle DBNull or null returns
                if (result == null || result is DBNull)
                {
                    return default;
                }

                // Handle underlying types for Nullable<T> (e.g. int?)
                Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

                // Safely convert SQL Server types (e.g. decimal to int/long/Guid)
                return (T)Convert.ChangeType(result, targetType);
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
        /// Executes a query within an optional transaction and maps the first matching record, returning null if not found.
        /// </summary>
        protected T? ExecuteSingle<T>(SqlConnection connection, SqlTransaction? transaction, Func<SqlDataReader, T> mapFunction, IQueryWithParameters queryWithParameters) where T : class
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }

                using SqlCommand cmd = new SqlCommand(queryWithParameters.Script, connection, transaction);

                if (queryWithParameters.Parameters != null)
                {
                    foreach (var param in queryWithParameters.Parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }
                using SqlDataReader reader = cmd.ExecuteReader();
                return reader.Read() ? mapFunction(reader) : null;
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
        /// Executes a query within an optional transaction and maps all matching records to a list.
        /// </summary>
        protected List<T> ExecuteList<T>(SqlConnection connection, SqlTransaction? transaction, Func<SqlDataReader, T> mapFunction, IQueryWithParameters queryWithParameters)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }

                using SqlCommand cmd = new SqlCommand(queryWithParameters.Script, connection, transaction);

                if (queryWithParameters.Parameters != null)
                {
                    foreach (var param in queryWithParameters.Parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }

                var results = new List<T>();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(mapFunction(reader));
                }

                return results;
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
        /// Checks if a record exists based on a query
        /// </summary>
        protected bool RecordExists(SqlConnection connection, SqlTransaction transaction, IQueryWithParameters queryWithParameters)
        {
            try
            {
                int count = ExecuteScalar<int>(connection, transaction, queryWithParameters);
                return count > 0;
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
    }
}
    
