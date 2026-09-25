using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Domain.Entities;

namespace TraderView.Application.Specifications.Instruments
{
    /// <summary>
    /// Specification to select an Instrument by its ConId
    /// </summary>
    public class GetInstrumentByConIdSpecification : ISpecification<Instrument>
    {
        public Expression<Func<Instrument, bool>>? Criteria { get; }
        public List<Expression<Func<Instrument, object>>> Includes { get; } = new();
        public List<string> IncludeStrings { get; } = new();
        public List<(Expression<Func<Instrument, object>> KeySelector, bool IsDescending)> OrderBys { get; } = new();
        public int? Take { get; } = null;
        public int? Skip { get; } = null;
        public bool IsPagingEnabled { get; } = false;

        /// <summary>
        /// Create a new specification that matches the provided ConId
        /// </summary>
        public GetInstrumentByConIdSpecification(string conId)
        {
            Criteria = i => i.ConId == conId;
        }
    }
}
