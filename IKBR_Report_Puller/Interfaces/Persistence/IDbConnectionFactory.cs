using System.Data;

namespace TraderView.Application.Interfaces.Persistence;

/// <summary>
/// Abstracts creation of database connections for repository usage.
/// Implementations should return a closed IDbConnection instance ready to be opened by the caller.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Create a new IDbConnection instance (closed).
    /// Caller is responsible for opening/disposing the connection.
    /// </summary>
    IDbConnection CreateConnection();
}
