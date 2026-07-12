namespace IKBR_Report_Puller.Domain
{
    /// <summary>
    /// Represents an item in a list
    /// </summary>
    public class ListItem
    {
        public int Id { get; set; }
        public string ListName { get; set; } = string.Empty;
        public string Item { get; set; } = string.Empty;
    }
}
