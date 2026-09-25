using TraderView.Domain.Entities;

namespace TraderView.Application.Specifications.Notes
{
    /// <summary>
    /// Specification to select Notes by TradeTypeId (infrastructure-level)
    /// </summary>
    public class NoteByTradeTypeSpecification : BaseSpecification<Note>
    {
        /// <summary>
        /// Create a new specification that matches the provided trade type id
        /// </summary>
        public NoteByTradeTypeSpecification(int tradeTypeId)
        {
            Criteria = n => n.TradeTypeId == tradeTypeId;
            ApplyOrdering(n => n.EntryDate, isDescending: true);
        }
    }
}
