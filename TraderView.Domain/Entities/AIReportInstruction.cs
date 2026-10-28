namespace TraderView.Domain.Entities
{
    public class AIReportInstruction
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string Instruction { get; set; } = null!;

        public virtual ICollection<AIReport> AIReports { get; set; } = new List<AIReport>();
    }
}
