using System;
using TraderView.Domain.Entities;

namespace TraderView.Application.Specifications.EquitySummaries
{
    /// <summary>
    /// Specification to select an EquitySummary by account ID and report date
    /// </summary>
    public class EquitySummaryByAccountAndDateSpecification : BaseSpecification<EquitySummary>
    {
        /// <summary>
        /// Create a new specification that matches the provided account ID and report date
        /// </summary>
        public EquitySummaryByAccountAndDateSpecification(string accountId, DateTime reportDate)
        {
            Criteria = es => es.AccountId == accountId && es.ReportDate.Date == reportDate.Date;
        }
    }
}
