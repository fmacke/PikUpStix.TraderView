using Microsoft.Identity.Client;
using System;

namespace IKBR_Report_Puller.Domain
{
    public class OpenPosition
    {
        public string AccountId { get; set; }
        public int PositionID { get; set; }
        public string AcctAlias { get; set; }
        public string Model { get; set; }
        public string Currency { get; set; }
        public decimal? FxRateToBase { get; set; }
        public string AssetCategory { get; set; }
        public string SubCategory { get; set; }
        public string Symbol { get; set; }
        public string Description { get; set; }
        public long? Conid { get; set; }
        public string SecurityID { get; set; }
        public string SecurityIDType { get; set; }
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
        public DateTime? ReportDate { get; set; }
        public decimal? Position { get; set; }
        public decimal? MarkPrice { get; set; }
        public decimal? PositionValue { get; set; }
        public decimal? OpenPrice { get; set; }
        public decimal? CostBasisPrice { get; set; }
        public decimal? CostBasisMoney { get; set; }
        public decimal? PercentOfNAV { get; set; }
        public decimal? FifoPnlUnrealized { get; set; }
        public string Side { get; set; }
        public string LevelOfDetail { get; set; }
        public string OpenDateTime { get; set; }
        public string HoldingPeriodDateTime { get; set; }
        public DateTime? VestingDate { get; set; }
        public string Code { get; set; }
        public long? OriginatingOrderID { get; set; }
        public long? OriginatingTransactionID { get; set; }
        public decimal? AccruedInt { get; set; }
        public string SerialNumber { get; set; }
        public string DeliveryType { get; set; }
        public string CommodityType { get; set; }
        public decimal? Fineness { get; set; }
        public decimal? Weight { get; set; }
    }
}
