using System;

namespace IKBR_Report_Puller.Domain
{
    /// <summary>
    /// Represents a note associated with a position or trade execution
    /// </summary>
    public class Note
    {
        public int Id { get; set; }
        public int PositionId { get; set; }
        public int? TradeExecutionId { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime EntryDate { get; set; }
        public int TradeTypeId { get; set; }
    }
}
