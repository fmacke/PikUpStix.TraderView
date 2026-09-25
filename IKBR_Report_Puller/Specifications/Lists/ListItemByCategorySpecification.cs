using TraderView.Domain.Entities;

namespace TraderView.Application.Specifications.List
{
    /// <summary>
    /// Specification to select active ListItems by Category
    /// </summary>
    public class ListItemByCategorySpecification : BaseSpecification<ListItem>
    {
        public ListItemByCategorySpecification(string category)
        {
            Criteria = li => li.Category == category && li.IsActive == true;
            ApplyOrdering(li => li.Name, isDescending: false);
        }
    }
}
