namespace TraderView.Domain.Entities
{
    public class TradeConfirm
    {
        public int Id { get; set; }
        public int PositionId { get; set; }
        public string Currency { get; set; }
        public string AssetCategory { get; set; }
        public string Symbol { get; set; }
        public string Description { get; set; }
        public string Conid { get; set; }
        public string SecurityIDType { get; set; }
        public string AccountId { get; set; }
        public string Cusip { get; set; }
        public string Isin { get; set; }
        public string Figi { get; set; }
        public string ListingExchange { get; set; }
        public string UnderlyingConid { get; set; }
        public string UnderlyingSymbol { get; set; }
        public string UnderlyingSecurityID { get; set; }
        public string UnderlyingListingExchange { get; set; }
        public string Issuer { get; set; }
        public string IssuerCountryCode { get; set; }
        public int? Multiplier { get; set; }
        public decimal? Strike { get; set; }
        public string Expiry { get; set; }
        public string PutCall { get; set; }
        public decimal? PrincipalAdjustFactor { get; set; }
        public long? TradeID { get; set; }
        public DateTime TradeDate { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime SettleDateTarget { get; set; }
        public string TransactionType { get; set; }
        public string Exchange { get; set; }
        public decimal Quantity { get; set; }
        public decimal TradePrice { get; set; }
        public decimal ClosePrice { get; set; }
        public decimal Cost { get; set; }
        public string LevelOfDetail { get; set; }
        public decimal? TradeMoney { get; set; }
        public decimal? Proceeds { get; set; }
        public decimal? Taxes { get; set; }
        public decimal? Commission { get; set; }
        public string CommissionCurrency { get; set; }
        public decimal? NetCash { get; set; }        
        public string Notes { get; set; }
        public decimal? Amount { get; set; }
        public decimal? FifoPnlRealized { get; set; }
        public decimal? OrigTradePrice { get; set; }
        public string OrigTradeDate { get; set; }
        public string OrigTradeID { get; set; }
        public long? OrigOrderID { get; set; }
        public long? OrigTransactionID { get; set; }
        public string ClearingFirmID { get; set; }
        public long? OrderID { get; set; }
        public string IbExecID { get; set; }
        public string BuySell { get; set; }
        public string OpenCloseIndicator { get; set; }
    }
}
