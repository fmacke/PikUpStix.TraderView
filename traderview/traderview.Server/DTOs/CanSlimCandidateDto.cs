namespace traderview.Server.Dtos
{
    public class CanSlimCandidateDto
    {
        public int Id { get; set; }
        public int CanSlimScreenerSnapshotId { get; set; }
        public string Symbol { get; set; } = null!;
        public string? Exchange { get; set; }
        public string? CompanyName { get; set; }
        public string? Sector { get; set; }
        public string? Industry { get; set; }
        public decimal Price { get; set; }
        public decimal Volume { get; set; }
        public decimal MarketCap { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime EvaluationDateUtc { get; set; }
        public bool PassesBoth { get; set; }
    }
}
