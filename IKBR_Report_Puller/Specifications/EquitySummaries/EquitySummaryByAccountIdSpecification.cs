using TraderView.Domain.Entities;

namespace TraderView.Application.Specifications.EquitySummaries
{
    /// <summary>
    /// Specification to select all EquitySummaries for a specific account
    /// </summary>
    public class EquitySummaryByAccountIdSpecification : BaseSpecification<EquitySummary>
    {
        /// <summary>
        /// Create a new specification that matches the provided account ID
        /// </summary>
        public EquitySummaryByAccountIdSpecification(string accountId)
        {
            Criteria = es => es.AccountId == accountId;
            ApplyOrdering(es => es.ReportDate, isDescending: true);
        }
    }
}
