namespace traderview.Server.DTOs
{
    /// <summary>
    /// Request DTO for exporting multiple company checklist reports to Excel.
    /// </summary>
    public class ExportChecklistReportsRequest
    {
        /// <summary>
        /// List of stock symbols to fetch and export.
        /// </summary>
        public List<string> Symbols { get; set; } = new();
    }
}
