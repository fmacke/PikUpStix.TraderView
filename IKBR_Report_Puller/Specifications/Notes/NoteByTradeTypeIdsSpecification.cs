using System.Collections.Generic;
using TraderView.Domain.Entities;

namespace TraderView.Application.Specifications.Notes
{
    /// <summary>
    /// Specification to select Notes by a set of trade type ids (infrastructure-level)
    /// </summary>
    public class NoteByTradeTypeIdsSpecification : BaseSpecification<Note>
    {
        public NoteByTradeTypeIdsSpecification(IEnumerable<int> tradeTypeIds)
        {
            // match notes whose TradeTypeId is non-null and in the provided list
            Criteria = n => n.TradeTypeId != null && tradeTypeIds.Contains(n.TradeTypeId.Value);
            ApplyOrdering(n => n.EntryDate, isDescending: true);
        }
    }
}
