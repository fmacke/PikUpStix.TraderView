using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System;

namespace TraderView.Domain.Entities
{
    /// <summary>
    /// Represents a note associated with a position or trade execution
    /// </summary>
    public class Note
    {
        public int Id { get; set; }

        public int PositionId { get; set; }

        public int? TradeExecutionId { get; set; }

        public int? TradeTypeId { get; set; }

        public string Comment { get; set; } = null!;

        public DateTime EntryDate { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual Position Position { get; set; } = null!;

    }
}
