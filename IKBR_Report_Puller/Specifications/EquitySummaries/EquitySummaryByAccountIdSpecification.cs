using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Domain.Entities;

namespace TraderView.Application.Specifications.EquitySummaries
{
    /// <summary>
    /// Specification to select all EquitySummaries for a specific account
    /// </summary>
    public class EquitySummaryByAccountIdSpecification : ISpecification<EquitySummary>
    {
        public Expression<Func<EquitySummary, bool>>? Criteria { get; }
        public List<Expression<Func<EquitySummary, object>>> Includes { get; } = new();
        public List<string> IncludeStrings { get; } = new();
        public List<(Expression<Func<EquitySummary, object>> KeySelector, bool IsDescending)> OrderBys { get; } = new();
        public int? Take { get; } = null;
        public int? Skip { get; } = null;
        public bool IsPagingEnabled { get; } = false;

        /// <summary>
        /// Create a new specification that matches the provided account ID
        /// </summary>
        public EquitySummaryByAccountIdSpecification(string accountId)
        {
            Criteria = es => es.AccountId == accountId;
            OrderBys.Add((es => es.ReportDate, true)); // Descending by report date
        }
    }
}
