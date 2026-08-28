
using System;
using System.Collections.Generic;

namespace TraderView.Domain.Entities;

public partial class StrategyStage
{
    public int Id { get; set; }

    public int StrategyId { get; set; }

    public int? CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? DataMapping { get; set; }

    public string? Notes { get; set; }

    public virtual ListItem? Category { get; set; }

    public virtual Strategy Strategy { get; set; } = null!;

    public virtual ICollection<StrategyStageStep> StrategyStageSteps { get; set; } = new List<StrategyStageStep>();
}
