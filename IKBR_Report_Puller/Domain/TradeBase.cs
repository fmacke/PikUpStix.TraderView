namespace IKBR_Report_Puller.Domain
{
    public class TradeBase
    {
        private decimal _quantity;
        public decimal Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                TradeType = _quantity > 0 ? TradeType.Long : TradeType.Short;
            }
        }
        public long IbOrderID { get; set; }
        public string Symbol { get; set; }
        public int InstrumentId { get; set; }
        public string Currency { get; set; }
        public string SecurityId { get; set; }
        public decimal AveragePrice { get; set; }
        public DateTime TradeDate { get; set; }
        public TradeType TradeType { get; private set; } // Made setter private to ensure it is only set internally
        public bool IsLong => TradeType == TradeType.Long;
        public bool IsShort => TradeType == TradeType.Short;
    }
}
