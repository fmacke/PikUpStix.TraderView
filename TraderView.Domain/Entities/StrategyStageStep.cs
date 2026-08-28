using System;
using System.Collections.Generic;

namespace TraderView.Domain.Entities;

public partial class StrategyStageStep
{
    public int Id { get; set; }

    public int StageId { get; set; }

    public int? CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? DataMapping { get; set; }

    public string? Notes { get; set; }

    public virtual ListItem? Category { get; set; }

    public virtual StrategyStage Stage { get; set; } = null!;

    public virtual ICollection<StrategyStageStepsRule> StrategyStageStepsRules { get; set; } = new List<StrategyStageStepsRule>();
}
