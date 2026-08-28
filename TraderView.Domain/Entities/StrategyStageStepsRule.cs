using System;
using System.Collections.Generic;

namespace TraderView.Domain.Entities;

public partial class StrategyStageStepsRule
{
    public int Id { get; set; }

    public int StepId { get; set; }

    public string Description { get; set; } = null!;

    public decimal? NumberValue1 { get; set; }

    public decimal? NumberValue2 { get; set; }

    public decimal? NumberValue3 { get; set; }

    public bool? IsBinaryChoice { get; set; }

    public virtual StrategyStageStep Step { get; set; } = null!;
}
