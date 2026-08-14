using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PikUpStix.TraderView.Data.Scripts.DataComms.TradeExecutions.Changes
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
                        VALUES (@positionId, @ibOrderID, @ibexecID, @symbol, @tradeDate, @dateTime, @quantity, @tradePrice, @currency, 
                        @conid, @tradeID, @fifoPnlRealized, @ibCommission, @assetCategory, @description, @securityIDType, @cusip,
                        @accountId, @isin, @figi, @listingExchange, @UnderlyingConid, @UnderlyingSymbol, @UnderlyingSecurityID, @UnderlyingListingExchange,
                        @Issuer, @IssuerCountryCode, @Multiplier, @Strike, @Expiry, @PutCall, @PrincipalAdjustFactor, @TransactionType,
                        @Exchange, @Proceeds, @ibCommissionCurrency, @NetCash, @Cost, @OrigTradePrice, @OrigTradeDate, @OrigTradeID, @OrigOrderID,
                        @OrigTransactionID, @ClearingFirmID, @BuySell, @OpenCloseIndicator)";
        }
    }
    public class TradeExecutionInsert
    {
        public string Script()
        {
            return @"
                INSERT INTO [dbo].[TradeExecutions]
                ([PositionID], [symbol], [securityID], [tradeID], [dateTime], [tradeDate], [quantity], [tradePrice], [ibCommission],
                 [ibCommissionCurrency], [closePrice], [cost], [fifoPnlRealized], [buySell], [transactionID], [ibExecID],
                 [brokerageOrderID], [exchOrderId], [extExecID], [orderType], [traderID], [currency], [description],
                 [conid], [taxes], [assetCategory], [expiry], [transactionType], [exchange], [proceeds], [netCash],
                 [mtmPnl], [origTradePrice], [origTradeDate], [origTradeID], [origOrderID], [origTransactionID],
                 [ibOrderID], [openDateTime], [initialInvestment], [accountId], [acctAlias], [model], [fxRateToBase],
                 [subCategory], [securityIDType], [cusip], [isin], [figi], [listingExchange], [underlyingConid],
                 [underlyingSymbol], [underlyingSecurityID], [underlyingListingExchange], [issuer], [issuerCountryCode],
                 [multiplier], [relatedTradeID], [strike], [reportDate], [putCall], [principalAdjustFactor],
                 [settleDateTarget], [tradeMoney], [openCloseIndicator], [notes], [clearingFirmID], [relatedTransactionID],
                 [rtn], [orderReference], [volatilityOrderLink], [orderTime], [holdingPeriodDateTime], [whenRealized],
                 [whenReopened], [levelOfDetail], [changeInPrice], [changeInQuantity], [isAPIOrder], [accruedInt],
                 [positionActionID], [serialNumber], [deliveryType], [commodityType], [fineness], [weight])
                VALUES
                (@positionId, @symbol, @securityID, @tradeID, @dateTime, @tradeDate, @quantity, @tradePrice, @ibCommission,
                 @ibCommissionCurrency, @closePrice, @cost, @fifoPnlRealized, @buySell, @transactionID, @ibExecID,
                 @brokerageOrderID, @exchOrderId, @extExecID, @orderType, @traderID, @currency, @description,
                 @conid, @taxes, @assetCategory, @expiry, @transactionType, @exchange, @proceeds, @netCash,
                 @mtmPnl, @origTradePrice, @origTradeDate, @origTradeID, @origOrderID, @origTransactionID,
                 @ibOrderID, @openDateTime, @initialInvestment, @accountId, @acctAlias, @model, @fxRateToBase,
                 @subCategory, @securityIDType, @cusip, @isin, @figi, @listingExchange, @underlyingConid,
                 @underlyingSymbol, @underlyingSecurityID, @underlyingListingExchange, @issuer, @issuerCountryCode,
                 @multiplier, @relatedTradeID, @strike, @reportDate, @putCall, @principalAdjustFactor,
                 @settleDateTarget, @tradeMoney, @openCloseIndicator, @notes, @clearingFirmID, @relatedTransactionID,
                 @rtn, @orderReference, @volatilityOrderLink, @orderTime, @holdingPeriodDateTime, @whenRealized,
                 @whenReopened, @levelOfDetail, @changeInPrice, @changeInQuantity, @isAPIOrder, @accruedInt,
                 @positionActionID, @serialNumber, @deliveryType, @commodityType, @fineness, @weight)";
        }
    }
}
