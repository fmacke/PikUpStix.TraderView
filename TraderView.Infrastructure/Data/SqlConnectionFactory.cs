using System.Data;
using Microsoft.Data.SqlClient;
using TraderView.Application.Interfaces.Persistence;

namespace TraderView.Infrastructure.Data;

/// <summary>
/// SqlConnectionFactory returns Microsoft.Data.SqlClient.SqlConnection instances using a provided connection string.
/// </summary>
public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <summary>
    /// Create a new SqlConnection instance (closed). Caller should open and dispose it.
    /// </summary>
    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
