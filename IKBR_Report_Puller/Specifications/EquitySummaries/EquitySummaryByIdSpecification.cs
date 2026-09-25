using TraderView.Domain.Entities;

namespace TraderView.Application.Specifications.EquitySummaries
{
    /// <summary>
    /// Specification to select an EquitySummary by its Id
    /// </summary>
    public class EquitySummaryByIdSpecification : BaseSpecification<EquitySummary>
    {
        /// <summary>
        /// Create a new specification that matches the provided equity summary id
        /// </summary>
        public EquitySummaryByIdSpecification(int id)
        {
            Criteria = es => es.Id == id;
        }
    }
}
