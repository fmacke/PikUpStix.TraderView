namespace TraderView.Application.Interfaces.Persistence;

/// <summary>
/// Generic repository interface for data access abstraction
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Get all entities
    /// </summary>
    Task<IReadOnlyList<T>> GetAllAsync();

    /// <summary>
    /// Get entity by ID
    /// </summary>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Get entities matching a specification
    /// </summary>
    Task<IReadOnlyList<T>> GetAsync(ISpecification<T> specification);

    /// <summary>
    /// Get a single entity matching a specification
    /// </summary>
    Task<T?> GetSingleAsync(ISpecification<T> specification);

    /// <summary>
    /// Get count of entities
    /// </summary>
    Task<int> CountAsync();

    /// <summary>
    /// Get count of entities matching a specification
    /// </summary>
    Task<int> CountAsync(ISpecification<T> specification);

    /// <summary>
    /// Add an entity
    /// </summary>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Add multiple entities
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Update an entity
    /// </summary>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Delete an entity
    /// </summary>
    Task DeleteAsync(T entity);

    /// <summary>
    /// Delete multiple entities
    /// </summary>
    Task DeleteRangeAsync(IEnumerable<T> entities);
}
