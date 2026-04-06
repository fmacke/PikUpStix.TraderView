namespace IKBR_Report_Puller.Domain
{
    /// <summary>
    /// Represents an instrument in the trading system
    /// </summary>
    public class Instrument
    {
        public int Id { get; set; }
        public string InstrumentName { get; set; }
        public string Provider { get; set; }
        public string DataName { get; set; }
        public string DataSource { get; set; }
        public string Format { get; set; }
        public string Frequency { get; set; }
        public double? ContractUnit { get; set; }
        public string ContractUnitType { get; set; }
        public string PriceQuotation { get; set; }
        public double? MinimumPriceFluctuation { get; set; }
        public string Currency { get; set; }
        public string ListingExchange { get; set; }
        public string ConId { get; set; }
    }
}
