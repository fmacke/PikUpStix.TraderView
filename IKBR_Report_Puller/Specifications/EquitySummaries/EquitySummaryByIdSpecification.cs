using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Domain.Entities;

namespace TraderView.Application.Specifications.EquitySummaries
{
    /// <summary>
    /// Specification to select an EquitySummary by its Id
    /// </summary>
    public class EquitySummaryByIdSpecification : ISpecification<EquitySummary>
    {
        public Expression<Func<EquitySummary, bool>>? Criteria { get; }
        public List<Expression<Func<EquitySummary, object>>> Includes { get; } = new();
        public List<string> IncludeStrings { get; } = new();
        public List<(Expression<Func<EquitySummary, object>> KeySelector, bool IsDescending)> OrderBys { get; } = new();
        public int? Take { get; } = null;
        public int? Skip { get; } = null;
        public bool IsPagingEnabled { get; } = false;

        /// <summary>
        /// Create a new specification that matches the provided equity summary id
        /// </summary>
        public EquitySummaryByIdSpecification(int id)
        {
            Criteria = es => es.Id == id;
        }
    }
}
