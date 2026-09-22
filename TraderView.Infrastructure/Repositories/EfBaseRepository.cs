using Microsoft.EntityFrameworkCore;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Infrastructure.DbContexts;

namespace TraderView.Infrastructure.Repositories
{
    /// <summary>
    /// EF Core implementation of the generic repository interface
    /// </summary>
    public class EfBaseRepository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _db;

        public EfBaseRepository(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _db.Set<T>().AddAsync(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public virtual async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _db.Set<T>().AddRangeAsync(entities);
            await _db.SaveChangesAsync();
        }

        public virtual async Task<int> CountAsync()
        {
            return await _db.Set<T>().CountAsync();
        }

        public virtual async Task<int> CountAsync(ISpecification<T> specification)
        {
            var query = ApplySpecification(_db.Set<T>().AsQueryable(), specification);
            return await query.CountAsync();
        }

        public virtual async Task DeleteAsync(T entity)
        {
            _db.Set<T>().Remove(entity);
            await _db.SaveChangesAsync();
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            _db.Set<T>().RemoveRange(entities);
            await _db.SaveChangesAsync();
        }

        public virtual async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await _db.Set<T>().ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _db.Set<T>().FindAsync(id);
        }

        public virtual async Task<IReadOnlyList<T>> GetAsync(ISpecification<T> specification)
        {
            var query = ApplySpecification(_db.Set<T>().AsQueryable(), specification);
            return await query.ToListAsync();
        }

        public virtual async Task<T?> GetSingleAsync(ISpecification<T> specification)
        {
            var query = ApplySpecification(_db.Set<T>().AsQueryable(), specification);
            return await query.FirstOrDefaultAsync();
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _db.Set<T>().Update(entity);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Applies a specification object to an IQueryable
        /// </summary>
        protected IQueryable<T> ApplySpecification(IQueryable<T> inputQuery, ISpecification<T>? specification)
        {
            var query = inputQuery;
            if (specification == null)
                return query;

            if (specification.Criteria != null)
            {
                query = query.Where(specification.Criteria);
            }

            if (specification.Includes != null)
            {
                foreach (var include in specification.Includes)
                {
                    query = query.Include(include);
                }
            }

            if (specification.IncludeStrings != null)
            {
                foreach (var includeString in specification.IncludeStrings)
                {
                    query = query.Include(includeString);
                }
            }

            if (specification.OrderBys != null && specification.OrderBys.Count > 0)
            {
                IOrderedQueryable<T>? ordered = null;
                for (int i = 0; i < specification.OrderBys.Count; i++)
                {
                    var (keySelector, isDesc) = specification.OrderBys[i];
                    if (i == 0)
                    {
                        ordered = isDesc ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
                    }
                    else
                    {
                        ordered = isDesc ? ordered!.ThenByDescending(keySelector) : ordered!.ThenBy(keySelector);
                    }
                }

                if (ordered != null)
                    query = ordered;
            }

            if (specification.IsPagingEnabled)
            {
                if (specification.Skip.HasValue)
                    query = query.Skip(specification.Skip.Value);
                if (specification.Take.HasValue)
                    query = query.Take(specification.Take.Value);
            }
            else
            {
                if (specification.Skip.HasValue)
                    query = query.Skip(specification.Skip.Value);
                if (specification.Take.HasValue)
                    query = query.Take(specification.Take.Value);
            }

            return query;
        }
    }
}
