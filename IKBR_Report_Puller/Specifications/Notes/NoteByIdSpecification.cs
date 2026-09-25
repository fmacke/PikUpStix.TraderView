using TraderView.Domain.Entities;

namespace TraderView.Application.Specifications.Notes
{
    /// <summary>
    /// Specification to select a Note by its Id (infrastructure-level convenience implementation)
    /// </summary>
    public class NoteByIdSpecification : BaseSpecification<Note>
    {
        /// <summary>
        /// Create a new specification that matches the provided note id
        /// </summary>
        public NoteByIdSpecification(int id)
        {
            Criteria = n => n.Id == id;
            ApplyOrdering(n => n.EntryDate, isDescending: true);
        }
    }
}
