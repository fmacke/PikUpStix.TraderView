namespace TraderView.Domain.Entities
{
    public class TradeExecution
    {
        public int Id { get; set; }

        public int? PositionId { get; set; }

        public string? Symbol { get; set; }

        public string? SecurityId { get; set; }

        public long? TradeId { get; set; }

        public DateTime DateTime { get; set; }

        public DateTime TradeDate { get; set; }

        public decimal? Quantity { get; set; }

        public decimal? TradePrice { get; set; }

        public decimal? IbCommission { get; set; }

        public string? IbCommissionCurrency { get; set; }

        public decimal? ClosePrice { get; set; }

        public decimal? Cost { get; set; }

        public decimal? FifoPnlRealized { get; set; }

        public string? BuySell { get; set; }

        public long? TransactionId { get; set; }

        public string? IbExecId { get; set; }

        public string? BrokerageOrderId { get; set; }

        public string? ExchOrderId { get; set; }

        public string? ExtExecId { get; set; }

        public string? OrderType { get; set; }

        public string? TraderId { get; set; }

        public string? Currency { get; set; }

        public string? Description { get; set; }

        public string? Conid { get; set; }

        public decimal? Taxes { get; set; }

        public string? AssetCategory { get; set; }

        public string? Expiry { get; set; }

        public string? TransactionType { get; set; }

        public string? Exchange { get; set; }

        public decimal? Proceeds { get; set; }

        public decimal? NetCash { get; set; }

        public decimal? MtmPnl { get; set; }

        public decimal? OrigTradePrice { get; set; }

        public string? OrigTradeDate { get; set; }

        public string? OrigTradeId { get; set; }

        public long? OrigOrderId { get; set; }

        public long? OrigTransactionId { get; set; }

        public long? IbOrderId { get; set; }

        public string? OpenDateTime { get; set; }

        public decimal? InitialInvestment { get; set; }

        public string? AccountId { get; set; }

        public string? AcctAlias { get; set; }

        public string? Model { get; set; }

        public decimal? FxRateToBase { get; set; }

        public string? SubCategory { get; set; }

        public string? SecurityIdtype { get; set; }

        public string? Cusip { get; set; }

        public string? Isin { get; set; }

        public string? Figi { get; set; }

        public string? ListingExchange { get; set; }

        public string? UnderlyingConid { get; set; }

        public string? UnderlyingSymbol { get; set; }

        public string? UnderlyingSecurityId { get; set; }

        public string? UnderlyingListingExchange { get; set; }

        public string? Issuer { get; set; }

        public string? IssuerCountryCode { get; set; }

        public int? Multiplier { get; set; }

        public string? RelatedTradeId { get; set; }

        public decimal? Strike { get; set; }

        public string? ReportDate { get; set; }

        public string? PutCall { get; set; }

        public decimal? PrincipalAdjustFactor { get; set; }

        public string? SettleDateTarget { get; set; }

        public decimal? TradeMoney { get; set; }

        public string? OpenCloseIndicator { get; set; }

        public string? Notes { get; set; }

        public string? ClearingFirmId { get; set; }

        public string? RelatedTransactionId { get; set; }

        public string? Rtn { get; set; }

        public string? OrderReference { get; set; }

        public string? VolatilityOrderLink { get; set; }

        public string? OrderTime { get; set; }

        public string? HoldingPeriodDateTime { get; set; }

        public string? WhenRealized { get; set; }

        public string? WhenReopened { get; set; }

        public string? LevelOfDetail { get; set; }

        public decimal? ChangeInPrice { get; set; }

        public decimal? ChangeInQuantity { get; set; }

        public string? IsApiorder { get; set; }

        public decimal? AccruedInt { get; set; }

        public string? PositionActionId { get; set; }

        public string? SerialNumber { get; set; }

        public string? DeliveryType { get; set; }

        public string? CommodityType { get; set; }

        public decimal? Fineness { get; set; }

        public decimal? Weight { get; set; }

        public virtual Position? Position { get; set; }
    }
}
