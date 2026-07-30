namespace traderview.Server.DTOs
{
    /// <summary>
    /// DTO for creating a new note
    /// </summary>
    public class CreateNoteDto
    {
        public int PositionId { get; set; }
        public int? TradeExecutionId { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime EntryDate { get; set; }
        public int TradeTypeId { get; set; }
    }

    /// <summary>
    /// DTO for returning note data
    /// </summary>
    public class NoteDto
    {
        public int Id { get; set; }
        public int PositionId { get; set; }
        public int? TradeExecutionId { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime EntryDate { get; set; }
        public int TradeTypeId { get; set; }
    }
}
