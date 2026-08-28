using System;
using System.Collections.Generic;

namespace TraderView.Domain.Entities;

public partial class Strategy
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<StrategyStage> StrategyStages { get; set; } = new List<StrategyStage>();
}
