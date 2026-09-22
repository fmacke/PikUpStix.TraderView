using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Domain.Entities;

namespace TraderView.Application.Specifications
{
    /// <summary>
    /// Specification to select active ListItems by Category
    /// </summary>
    public class ListItemByCategorySpecification : ISpecification<ListItem>
    {
        public Expression<Func<ListItem, bool>>? Criteria { get; }
        public List<Expression<Func<ListItem, object>>> Includes { get; } = new();
        public List<string> IncludeStrings { get; } = new();
        public List<(Expression<Func<ListItem, object>> KeySelector, bool IsDescending)> OrderBys { get; } = new();
        public int? Take { get; } = null;
        public int? Skip { get; } = null;
        public bool IsPagingEnabled { get; } = false;

        public ListItemByCategorySpecification(string category)
        {
            Criteria = li => li.Category == category && li.IsActive;
            OrderBys.Add((li => li.Name, false));
        }
    }
}
