namespace traderview.Server.DTOs
{
    public class TradeDto
    {
        public long Id { get; set; }
        public int InstrumentId { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public DateTime EntryDate { get; set; }
        public DateTime ExitDate { get; set; }
        public decimal EntryPrice { get; set; }
        public decimal ExitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal Pnl { get; set; }
        public string BuySell { get; set; } = string.Empty;
    }

    public class InstrumentDto
    {
        public int Id { get; set; }
        public string InstrumentName { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string DataName { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string? ListingExchange { get; set; }
    }

    public class TradeExecutionDto
    {
        public int Id { get; set; }
        public int InstrumentId { get; set; }
        public string? Symbol { get; set; }
        public long? TradeID { get; set; }
        public string? DateTime { get; set; }
        public DateTime? TradeDate { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? TradePrice { get; set; }
        public string? BuySell { get; set; }
        public decimal? FifoPnlRealized { get; set; }
        public decimal? IbCommission { get; set; }
    }

    public class CandlestickDto
    {
        public DateTime Date { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
    }

    public class TradeContextDto
    {
        public TradeDto Trade { get; set; } = new();
        public List<CandlestickDto> Candlesticks { get; set; } = new();
        public DateTime EntryDate { get; set; }
        public DateTime ExitDate { get; set; }
    }

    public class TradeDetailDto
    {
        public TradeDto Trade { get; set; } = new();
        public InstrumentDto Instrument { get; set; } = new();
        public List<TradeExecutionDto> Executions { get; set; } = new();
    }
}
