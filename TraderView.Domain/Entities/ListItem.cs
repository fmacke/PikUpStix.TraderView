namespace TraderView.Domain.Entities
{
    /// <summary>
    /// Represents an item in a list
    /// </summary>
    public class ListItem
    {
        public int Id { get; set; }
        public int? ParentListId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public string? Category { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Notes navigation removed to avoid creating an unintended shadow foreign key (ListItemId)
        // when the inverse navigation is not present on Note. Use explicit navigations on
        // the dependent entities (Note, StrategyStage, StrategyStageStep) when needed.

        //public virtual ICollection<StrategyStageStep> StrategyStageSteps { get; set; } = new List<StrategyStageStep>();

        //public virtual ICollection<StrategyStage> StrategyStages { get; set; } = new List<StrategyStage>();
    }
}
