using System;
using TraderView.Domain.Entities;

namespace TraderView.Application.Specifications.EquitySummaries
{
    /// <summary>
    /// Specification to select all EquitySummaries within a date range
    /// </summary>
    public class EquitySummaryByDateRangeSpecification : BaseSpecification<EquitySummary>
    {
        /// <summary>
        /// Create a new specification that matches equity summaries within the provided date range
        /// </summary>
        public EquitySummaryByDateRangeSpecification(DateTime startDate, DateTime endDate)
        {
            Criteria = es => es.ReportDate.Date >= startDate.Date && es.ReportDate.Date <= endDate.Date;
            ApplyOrdering(es => es.ReportDate, isDescending: true);
        }
    }
}
