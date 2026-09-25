using TraderView.Domain.Entities;

namespace TraderView.Application.Specifications.Notes
{
    /// <summary>
    /// Specification to select Notes by TradeExecutionId (infrastructure-level)
    /// </summary>
    public class NoteByTradeExecutionSpecification : BaseSpecification<Note>
    {
        /// <summary>
        /// Create a new specification that matches the provided trade execution id
        /// </summary>
        public NoteByTradeExecutionSpecification(int tradeExecutionId)
        {
            Criteria = n => n.TradeExecutionId == tradeExecutionId;
            ApplyOrdering(n => n.EntryDate, isDescending: true);
        }
    }
}
