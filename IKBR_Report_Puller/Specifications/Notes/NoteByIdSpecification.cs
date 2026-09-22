using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Domain.Entities;

namespace TraderView.Application.Specifications.Notes
{
    /// <summary>
    /// Specification to select a Note by its Id (infrastructure-level convenience implementation)
    /// </summary>
    public class NoteByIdSpecification : ISpecification<Note>
    {
        public Expression<Func<Note, bool>>? Criteria { get; }
        public List<Expression<Func<Note, object>>> Includes { get; } = new();
        public List<string> IncludeStrings { get; } = new();
        public List<(Expression<Func<Note, object>> KeySelector, bool IsDescending)> OrderBys { get; } = new();
        public int? Take { get; } = null;
        public int? Skip { get; } = null;
        public bool IsPagingEnabled { get; } = false;

        /// <summary>
        /// Create a new specification that matches the provided note id
        /// </summary>
        public NoteByIdSpecification(int id)
        {
            Criteria = n => n.Id == id;
            OrderBys.Add((n => n.EntryDate, true));
        }
    }
}
