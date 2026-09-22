using System.Linq.Expressions;
using Microsoft.Data.SqlClient;
using TraderView.Application.Interfaces.Persistence;

namespace TraderView.Infrastructure.Repositories;

/// <summary>
/// Generic repository implementation supporting specifications
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public partial class GenericRepository<T> : BaseRepository, IRepository<T> where T : class
{
    public GenericRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        var spec = new AllEntitiesSpecification<T>();
        return await GetAsync(spec);
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await Task.Run(() =>
        {
            try
            {
                using SqlConnection connection = (SqlConnection)ConnectionFactory.CreateConnection();
                connection.Open();

                string sql = $"SELECT * FROM [{typeof(T).Name}s] WHERE Id = @id";
                using SqlCommand cmd = new(sql, connection);
                cmd.Parameters.AddWithValue("@id", id);

                using SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return MapReaderToEntity(reader);
                }
                return null;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
                throw;
            }
        });
    }

    public async Task<IReadOnlyList<T>> GetAsync(ISpecification<T> specification)
    {
        return await Task.Run(() =>
        {
            try
            {
                using SqlConnection connection = (SqlConnection)ConnectionFactory.CreateConnection();
                connection.Open();

                var sql = BuildSqlQuery(specification);
                using SqlCommand cmd = new(sql, connection);
                AddParameters(cmd, specification);

                var results = new List<T>();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(MapReaderToEntity(reader));
                }

                return (IReadOnlyList<T>)results;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
                throw;
            }
        });
    }

    public async Task<T?> GetSingleAsync(ISpecification<T> specification)
    {
        return await Task.Run(() =>
        {
            try
            {
                using SqlConnection connection = (SqlConnection)ConnectionFactory.CreateConnection();
                connection.Open();

                var sql = BuildSqlQuery(specification);
                using SqlCommand cmd = new(sql, connection);
                AddParameters(cmd, specification);

                using SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return MapReaderToEntity(reader);
                }
                return null;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
                throw;
            }
        });
    }

    public async Task<int> CountAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                using SqlConnection connection = (SqlConnection)ConnectionFactory.CreateConnection();
                connection.Open();

                string sql = $"SELECT COUNT(*) FROM [{typeof(T).Name}s]";
                using SqlCommand cmd = new(sql, connection);

                var result = cmd.ExecuteScalar();
                return result != null && result != DBNull.Value ? (int)result : 0;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
                throw;
            }
        });
    }

    public async Task<int> CountAsync(ISpecification<T> specification)
    {
        return await Task.Run(() =>
        {
            try
            {
                using SqlConnection connection = (SqlConnection)ConnectionFactory.CreateConnection();
                connection.Open();

                var sql = BuildCountSqlQuery(specification);
                using SqlCommand cmd = new(sql, connection);
                AddParameters(cmd, specification);

                var result = cmd.ExecuteScalar();
                return result != null && result != DBNull.Value ? (int)result : 0;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
                throw;
            }
        });
    }

    public async Task<T> AddAsync(T entity)
    {
        try
        {
            using SqlConnection connection = (SqlConnection)ConnectionFactory.CreateConnection();
            connection.Open();

            throw new NotImplementedException("AddAsync requires entity-specific implementation");
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"Database error: {ex.Message}");
            throw;
        }
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        await Task.Run(() =>
        {
            try
            {
                using SqlConnection connection = (SqlConnection)ConnectionFactory.CreateConnection();
                connection.Open();

                throw new NotImplementedException("AddRangeAsync requires entity-specific implementation");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
                throw;
            }
        });
    }

    public async Task UpdateAsync(T entity)
    {
        await Task.Run(() =>
        {
            try
            {
                using SqlConnection connection = (SqlConnection)ConnectionFactory.CreateConnection();
                connection.Open();

                throw new NotImplementedException("UpdateAsync requires entity-specific implementation");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
                throw;
            }
        });
    }

    public async Task DeleteAsync(T entity)
    {
        await Task.Run(() =>
        {
            try
            {
                using SqlConnection connection = (SqlConnection)ConnectionFactory.CreateConnection();
                connection.Open();

                throw new NotImplementedException("DeleteAsync requires entity-specific implementation");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
                throw;
            }
        });
    }

    public async Task DeleteRangeAsync(IEnumerable<T> entities)
    {
        await Task.Run(() =>
        {
            try
            {
                using SqlConnection connection = (SqlConnection)ConnectionFactory.CreateConnection();
                connection.Open();

                throw new NotImplementedException("DeleteRangeAsync requires entity-specific implementation");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Database error: {ex.Message}");
                throw;
            }
        });
    }

    /// <summary>
    /// Build SQL query from specification - override in derived classes for entity-specific logic
    /// </summary>
    protected virtual string BuildSqlQuery(ISpecification<T> specification)
    {
        var tableName = typeof(T).Name + "s";
        var sql = $"SELECT * FROM [{tableName}]";

        if (specification.Criteria != null)
        {
            sql += BuildWhereClause(specification.Criteria);
        }

        if (specification.OrderBys.Any())
        {
            sql += " ORDER BY ";
            sql += string.Join(", ", specification.OrderBys.Select(o =>
                $"{ExpressionToSqlColumn(o.KeySelector)} {(o.IsDescending ? "DESC" : "ASC")}"));
        }

        if (specification.IsPagingEnabled)
        {
            if (specification.Skip.HasValue)
                sql += $" OFFSET {specification.Skip} ROWS";
            if (specification.Take.HasValue)
                sql += $" FETCH NEXT {specification.Take} ROWS ONLY";
        }

        return sql;
    }

    /// <summary>
    /// Build COUNT query from specification
    /// </summary>
    protected virtual string BuildCountSqlQuery(ISpecification<T> specification)
    {
        var tableName = typeof(T).Name + "s";
        var sql = $"SELECT COUNT(*) FROM [{tableName}]";

        if (specification.Criteria != null)
        {
            sql += BuildWhereClause(specification.Criteria);
        }

        return sql;
    }

    /// <summary>
    /// Convert LINQ expression to SQL WHERE clause - this is a simplified implementation
    /// </summary>
    protected virtual string BuildWhereClause(Expression<Func<T, bool>> criteria)
    {
        if (criteria == null)
            return string.Empty;

        var translator = new ExpressionToSqlTranslator();
        var (sql, parameters) = translator.Translate(criteria.Body);

        return string.IsNullOrWhiteSpace(sql) ? string.Empty : " WHERE " + sql;
    }

    /// <summary>
    /// Convert LINQ expression to SQL column name - override in subclasses
    /// </summary>
    protected virtual string ExpressionToSqlColumn(Expression<Func<T, object>> expression)
    {
        if (expression.Body is MemberExpression memberExpr)
        {
            return memberExpr.Member.Name;
        }
        return "Id";
    }

    /// <summary>
    /// Add parameters from specification to command - override if needed
    /// </summary>
    protected virtual void AddParameters(SqlCommand cmd, ISpecification<T> specification)
    {
        if (specification?.Criteria == null)
            return;

        var translator = new ExpressionToSqlTranslator();
        var (_, parameters) = translator.Translate(specification.Criteria.Body);

        for (int i = 0; i < parameters.Count; i++)
        {
            var p = parameters[i];
            cmd.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
        }
    }

    /// <summary>
    /// Map SqlDataReader to entity - override in derived classes for entity-specific mapping
    /// </summary>
    protected virtual T MapReaderToEntity(SqlDataReader reader)
    {
        throw new NotImplementedException("MapReaderToEntity must be implemented in derived classes");
    }
}

/// <summary>
/// Default specification for getting all entities
/// </summary>
internal class AllEntitiesSpecification<T> : ISpecification<T> where T : class
{
    public Expression<Func<T, bool>>? Criteria => null;
    public List<Expression<Func<T, object>>> Includes => new();
    public List<string> IncludeStrings => new();
    public List<(Expression<Func<T, object>> KeySelector, bool IsDescending)> OrderBys => new();
    public int? Take => null;
    public int? Skip => null;
    public bool IsPagingEnabled => false;
}
