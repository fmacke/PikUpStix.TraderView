using TraderView.Application.Mappers;
using TraderView.Domain.Entities;

namespace TraderView.Application.Features.TradeExecutions.Command.Create
{    
    public class InsertTradeConfirmationCommand : IQueryWithParameters
    {
        private readonly TradeConfirm _tradeConfirm;
        public InsertTradeConfirmationCommand(TradeConfirm tradeConfirm) { 
            _tradeConfirm = tradeConfirm;
        }
        public Dictionary<string, object> Parameters
        {
            get => MapToSql.GetTradeConfirmationParams(_tradeConfirm);
        }
        public string Script
        {
            get => @"
                        INSERT INTO dbo.TradeExecutions (PositionID, ibOrderID, ibexecID, symbol, tradeDate, dateTime, quantity, tradePrice, currency, conid,
                        tradeID, fifoPnlRealized, ibCommission, assetCategory, description, SecurityIDType, cusip, accountId, isin, figi, 
                        listingExchange, UnderlyingConid, UnderlyingSymbol, UnderlyingSecurityID, UnderlyingListingExchange,
                        Issuer, IssuerCountryCode, Multiplier, Strike, Expiry, PutCall, PrincipalAdjustFactor, TransactionType,
                        Exchange, Proceeds, ibCommissionCurrency, NetCash, Cost, OrigTradePrice, OrigTradeDate, OrigTradeID, OrigOrderID,
                        OrigTransactionID, ClearingFirmID, BuySell, openCloseIndicator, closePrice) 
                        OUTPUT INSERTED.Id
                        VALUES (@positionId, @ibOrderID, @ibexecID, @symbol, @tradeDate, @dateTime, @quantity, @tradePrice, @currency, 
                        @conid, @tradeID, @fifoPnlRealized, @ibCommission, @assetCategory, @description, @securityIDType, @cusip,
                        @accountId, @isin, @figi, @listingExchange, @UnderlyingConid, @UnderlyingSymbol, @UnderlyingSecurityID, @UnderlyingListingExchange,
                        @Issuer, @IssuerCountryCode, @Multiplier, @Strike, @Expiry, @PutCall, @PrincipalAdjustFactor, @TransactionType,
                        @Exchange, @Proceeds, @ibCommissionCurrency, @NetCash, @Cost, @OrigTradePrice, @OrigTradeDate, @OrigTradeID, @OrigOrderID,
                        @OrigTransactionID, @ClearingFirmID, @BuySell, @OpenCloseIndicator, @ClosePrice)";
        }
    }
}
