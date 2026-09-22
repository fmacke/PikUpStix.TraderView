using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Domain.Entities;

namespace TraderView.Infrastructure.Repositories
{
    /// <summary>
    /// Specification to select Notes within a date range (infrastructure-level)
    /// </summary>
    public class NoteByDateRangeSpecification : ISpecification<Note>
    {
        public Expression<Func<Note, bool>>? Criteria { get; }
        public List<Expression<Func<Note, object>>> Includes { get; } = new();
        public List<string> IncludeStrings { get; } = new();
        public List<(Expression<Func<Note, object>> KeySelector, bool IsDescending)> OrderBys { get; } = new();
        public int? Take { get; } = null;
        public int? Skip { get; } = null;
        public bool IsPagingEnabled { get; } = false;

        /// <summary>
        /// Create a new specification that matches notes in the given date range (inclusive)
        /// </summary>
        public NoteByDateRangeSpecification(DateTime startDate, DateTime endDate)
        {
            Criteria = n => n.EntryDate >= startDate && n.EntryDate <= endDate;
            OrderBys.Add((n => n.EntryDate, true));
        }
    }
}
