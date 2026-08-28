using System;
using System.Collections.Generic;

namespace TraderView.Domain.Entities;

public partial class HistoricalDatum
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public double OpenPrice { get; set; }

    public double ClosePrice { get; set; }

    public double LowPrice { get; set; }

    public double HighPrice { get; set; }

    public double Volume { get; set; }

    public double? Settle { get; set; }

    public double? OpenInterest { get; set; }

    public int InstrumentId { get; set; }

    public virtual Instrument Instrument { get; set; } = null!;
}
