namespace TraderView.Domain.Entities
{
    /// <summary>
    /// Represents an item in a list
    /// </summary>
    public class ListItem
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public string? Category { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();

        public virtual ICollection<StrategyStageStep> StrategyStageSteps { get; set; } = new List<StrategyStageStep>();

        public virtual ICollection<StrategyStage> StrategyStages { get; set; } = new List<StrategyStage>();
    }
}
