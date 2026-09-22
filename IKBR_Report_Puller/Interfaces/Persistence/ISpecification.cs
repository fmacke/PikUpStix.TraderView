using System.Linq.Expressions;

namespace TraderView.Application.Interfaces.Persistence;

/// <summary>
/// Specification pattern interface for encapsulating query logic
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface ISpecification<T> where T : class
{
    /// <summary>
    /// The criteria for filtering entities
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Include navigation properties
    /// </summary>
    List<Expression<Func<T, object>>> Includes { get; }

    /// <summary>
    /// String-based include for navigation properties that can't be expressed with LINQ
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// Ordering specification
    /// </summary>
    List<(Expression<Func<T, object>> KeySelector, bool IsDescending)> OrderBys { get; }

    /// <summary>
    /// Pagination skip count
    /// </summary>
    int? Take { get; }

    /// <summary>
    /// Pagination skip amount
    /// </summary>
    int? Skip { get; }

    /// <summary>
    /// Whether to apply pagination
    /// </summary>
    bool IsPagingEnabled { get; }
}
