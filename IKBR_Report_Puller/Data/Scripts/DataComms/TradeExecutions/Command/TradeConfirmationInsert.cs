namespace PikUpStix.TraderView.Data.Scripts.DataComms.TradeExecutions.Command
{    
    public class TradeConfirmationInsert
    {
        public string Script()
        {
            return @"
                        INSERT INTO dbo.TradeExecutions (PositionID, ibOrderID, ibexecID, symbol, tradeDate, dateTime, quantity, tradePrice, currency, conid,
                        tradeID, fifoPnlRealized, ibCommission, assetCategory, description, SecurityIDType, cusip, accountId, isin, figi, 
                        listingExchange, UnderlyingConid, UnderlyingSymbol, UnderlyingSecurityID, UnderlyingListingExchange,
                        Issuer, IssuerCountryCode, Multiplier, Strike, Expiry, PutCall, PrincipalAdjustFactor, TransactionType,
                        Exchange, Proceeds, ibCommissionCurrency, NetCash, Cost, OrigTradePrice, OrigTradeDate, OrigTradeID, OrigOrderID,
                        OrigTransactionID, ClearingFirmID, BuySell, openCloseIndicator) 
                        OUTPUT INSERTED.Id
                        VALUES (@positionId, @ibOrderID, @ibexecID, @symbol, @tradeDate, @dateTime, @quantity, @tradePrice, @currency, 
                        @conid, @tradeID, @fifoPnlRealized, @ibCommission, @assetCategory, @description, @securityIDType, @cusip,
                        @accountId, @isin, @figi, @listingExchange, @UnderlyingConid, @UnderlyingSymbol, @UnderlyingSecurityID, @UnderlyingListingExchange,
                        @Issuer, @IssuerCountryCode, @Multiplier, @Strike, @Expiry, @PutCall, @PrincipalAdjustFactor, @TransactionType,
                        @Exchange, @Proceeds, @ibCommissionCurrency, @NetCash, @Cost, @OrigTradePrice, @OrigTradeDate, @OrigTradeID, @OrigOrderID,
                        @OrigTransactionID, @ClearingFirmID, @BuySell, @OpenCloseIndicator)";
        }
    }
}
