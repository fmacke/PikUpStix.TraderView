namespace traderview.Server.DTOs
{
    /// <summary>
    /// DTO for creating a new note
    /// </summary>
    public class CreateNoteDto
    {
        public int? PositionId { get; set; }
        public int? TradeExecutionId { get; set; }
        public string Comment { get; set; } = string.Empty;
        public int? TradeTypeId { get; set; }
        public int? ErrorTypeId { get; set; }
        public int? ExitTypeId { get; set; }
        public decimal? Time { get; set; } = 0m;
    }


    /// <summary>
    /// DTO for updating an existing note
    /// </summary>
    public class UpdateNoteDto
    {
        public int Id { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public int? TradeTypeId { get; set; }
        public int? ErrorTypeId { get; set; }
        public int? ExitTypeId { get; set; }
        public decimal? Time { get; set; } = 0m;
    }

    /// <summary>
    /// DTO for returning note data
    /// </summary>
    public class NoteDto
    {
        public int Id { get; set; }
        public int? PositionId { get; set; }
        public int? TradeExecutionId { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } 
        public DateTime EntryDate { get; set; }
        public int? TradeTypeId { get; set; }
        public int? ErrorTypeId { get; set; }
        public int? ExitTypeId { get; set; }
        public decimal? Time { get; set; } = 0m;
        public string? Category { get; set; }
        public string? Name { get; set; }
    }
}
