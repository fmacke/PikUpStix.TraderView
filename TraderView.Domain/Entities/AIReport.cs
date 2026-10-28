namespace TraderView.Domain.Entities
{
    public class AIReport
    {
        public int Id { get; set; }
        public int ReportInstructionId { get; set; }
        public string ReportDetail { get; set; } = null!;

        public virtual AIReportInstruction ReportInstruction { get; set; } = null!;
    }
}
