namespace TraderView.Domain.Entities.FMP
{
    public class  CanSlimScreenerSnapshot
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual ICollection<CanSlimCandidate> CanSlimCandidates { get; set; } = new List<CanSlimCandidate>();

    }
}
