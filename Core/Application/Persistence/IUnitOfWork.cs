using System;
using System.Threading;
using System.Threading.Tasks;

namespace TraderView.Application.Interfaces.Persistence;

/// <summary>
/// Unit of Work abstraction to coordinate repositories and transactions.
/// Designed to be used with IRepository{T} and ISpecification{T} in an
/// onion-architecture/application-core setup.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Get a repository for the given entity type.
    /// Implementations may return a cached instance per unit-of-work scope.
    /// </summary>
    IRepository<T> Repository<T>() where T : class;

    /// <summary>
    /// Persist changes to the underlying data store.
    /// Returns the number of state entries written to the underlying store.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begin a new transaction scope if supported by the persistence implementation.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commit the active transaction.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollback the active transaction.
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
