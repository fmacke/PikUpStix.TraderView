using TraderView.Domain.Entities;

namespace TraderView.Application.Specifications.List
{
    /// <summary>
    /// Specification to select a ListItem by its Id
    /// </summary>
    public class ListItemByIdSpecification : BaseSpecification<ListItem>
    {
        public ListItemByIdSpecification(int id)
        {
            Criteria = li => li.Id == id;
            ApplyOrdering(li => li.Name, isDescending: false);
        }
    }
}
