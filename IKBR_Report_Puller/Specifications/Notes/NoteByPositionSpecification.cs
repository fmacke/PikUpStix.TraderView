using TraderView.Domain.Entities;

namespace TraderView.Application.Specifications.Notes
{
    /// <summary>
    /// Specification to select Notes by PositionId (infrastructure-level)
    /// </summary>
    public class NoteByPositionSpecification : BaseSpecification<Note>
    {
        /// <summary>
        /// Create a new specification that matches the provided position id
        /// </summary>
        public NoteByPositionSpecification(int positionId)
        {
            Criteria = n => n.PositionId == positionId;
            ApplyOrdering(n => n.EntryDate, isDescending: true);
        }
    }
}
