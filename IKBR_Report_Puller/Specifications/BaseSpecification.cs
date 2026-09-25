using System.Linq.Expressions;
using TraderView.Application.Interfaces.Persistence;

namespace TraderView.Application.Specifications
{
    /// <summary>
    /// Base specification class for implementing ISpecification<T>
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    public abstract class BaseSpecification<T> : ISpecification<T> where T : class
    {
        public Expression<Func<T, bool>>? Criteria { get; protected set; }
        public List<Expression<Func<T, object>>> Includes { get; } = new();
        public List<string> IncludeStrings { get; } = new();
        public List<(Expression<Func<T, object>> KeySelector, bool IsDescending)> OrderBys { get; } = new();
        public int? Take { get; protected set; }
        public int? Skip { get; protected set; }
        public bool IsPagingEnabled { get; protected set; }

        protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        protected virtual void AddInclude(string includeString)
        {
            IncludeStrings.Add(includeString);
        }

        protected virtual void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }

        protected virtual void ApplyOrdering(Expression<Func<T, object>> orderByExpression, bool isDescending = false)
        {
            OrderBys.Add((orderByExpression, isDescending));
        }
    }
}
