namespace TraderView.Domain.Entities
{
    /// <summary>
    /// Represents an item in a list
    /// </summary>
    public class ListItem
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
