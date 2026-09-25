using System;
using TraderView.Domain.Entities;

namespace TraderView.Application.Specifications.Notes
{
    /// <summary>
    /// Specification to select Notes within a date range (infrastructure-level)
    /// </summary>
    public class NoteByDateRangeSpecification : BaseSpecification<Note>
    {
        /// <summary>
        /// Create a new specification that matches notes in the given date range (inclusive)
        /// </summary>
        public NoteByDateRangeSpecification(DateTime startDate, DateTime endDate)
        {
            Criteria = n => n.EntryDate >= startDate && n.EntryDate <= endDate;
            ApplyOrdering(n => n.EntryDate, isDescending: true);
        }
    }
}
