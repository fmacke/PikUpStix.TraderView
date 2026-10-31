namespace traderview.Server.DTOs
{
    public class AIReportDto
    {
        public int Id { get; set; }
        public int ReportInstructionId { get; set; }
        public string ReportDetail { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAIReportDto
    {
        public int ReportInstructionId { get; set; }
        public string ReportDetail { get; set; } = null!;
    }

    public class UpdateAIReportDto
    {
        public int Id { get; set; }
        public int ReportInstructionId { get; set; }
        public string ReportDetail { get; set; } = null!;
    }
}
