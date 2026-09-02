using DocumentFormat.OpenXml.Math;
using System;
using System.Collections.Generic;
using System.Text;

namespace TraderView.Application.Features.TradeExecutions.Query.Get
{
    public class TransactionExecutionRecordExists : IQueryWithParameters
    {
        public TransactionExecutionRecordExists(string ibExecID) { IbExecID = ibExecID; }
        public string IbExecID { get; }
        public Dictionary<string, object> Parameters { get => new Dictionary<string, object> (); }
        public string Script { get => $"SELECT COUNT(*) FROM dbo.TradeExecutions WHERE ibExecID = '{ IbExecID }'"; }
    }
    public class TransactionExecutionRecordsByPositionsQuery : IQueryWithParameters
    {
        public TransactionExecutionRecordsByPositionsQuery(List<int> positionIds) { PositionIds = positionIds; }
        public List<int> PositionIds { get; }
        public Dictionary<string, object> Parameters { get => new Dictionary<string, object>(); }
        public string Script { get => $@"SELECT te.[Id]
                      ,te.[PositionId]
                      ,te.[symbol]
                      ,te.[securityID]
                      ,te.[tradeID]
                      ,te.[dateTime]
                      ,te.[tradeDate]
                      ,te.[quantity]
                      ,te.[tradePrice]
                      ,te.[ibCommission]
                      ,te.[ibCommissionCurrency]
                      ,te.[closePrice]
                      ,te.[cost]
                      ,te.[fifoPnlRealized]
                      ,te.[buySell]
                      ,te.[transactionID]
                      ,te.[ibExecID]
                      ,te.[brokerageOrderID]
                      ,te.[exchOrderId]
                      ,te.[extExecID]
                      ,te.[orderType]
                      ,te.[traderID]
                      ,te.[currency]
                      ,te.[description]
                      ,te.[conid]
                      ,te.[taxes]
                      ,te.[assetCategory]
                      ,te.[expiry]
                      ,te.[transactionType]
                      ,te.[exchange]
                      ,te.[proceeds]
                      ,te.[netCash]
                      ,te.[mtmPnl]
                      ,te.[origTradePrice]
                      ,te.[origTradeDate]
                      ,te.[origTradeID]
                      ,te.[origOrderID]
                      ,te.[origTransactionID]
                      ,te.[ibOrderID]
                      ,te.[openDateTime]
                      ,te.[initialInvestment]
                      ,te.[accountId]
                      ,te.[acctAlias]
                      ,te.[model]
                      ,te.[fxRateToBase]
                      ,te.[subCategory]
                      ,te.[securityIDType]
                      ,te.[cusip]
                      ,te.[isin]
                      ,te.[figi]
                      ,te.[listingExchange]
                      ,te.[underlyingConid]
                      ,te.[underlyingSymbol]
                      ,te.[underlyingSecurityID]
                      ,te.[underlyingListingExchange]
                      ,te.[issuer]
                      ,te.[issuerCountryCode]
                      ,te.[multiplier]
                      ,te.[relatedTradeID]
                      ,te.[strike]
                      ,te.[reportDate]
                      ,te.[putCall]
                      ,te.[principalAdjustFactor]
                      ,te.[settleDateTarget]
                      ,te.[tradeMoney]
                      ,te.[openCloseIndicator]
                      ,te.[notes]
                      ,te.[clearingFirmID]
                      ,te.[relatedTransactionID]
                      ,te.[rtn]
                      ,te.[orderReference]
                      ,te.[volatilityOrderLink]
                      ,te.[orderTime]
                      ,te.[holdingPeriodDateTime]
                      ,te.[whenRealized]
                      ,te.[whenReopened]
                      ,te.[levelOfDetail]
                      ,te.[changeInPrice]
                      ,te.[changeInQuantity]
                      ,te.[isAPIOrder]
                      ,te.[accruedInt]
                      ,te.[positionActionID]
                      ,te.[serialNumber]
                      ,te.[deliveryType]
                      ,te.[commodityType]
                      ,te.[fineness]
                      ,te.[weight]
                      ,p.[InstrumentId]
                      ,p.[Status]
                      ,p.[OpenDate]
                      ,p.[CloseDate] 
                  FROM [TradingBE].[dbo].[TradeExecutions] te
                  inner join [TradingBE].[dbo].[Positions] p on te.PositionId = p.Id 
                  WHERE te.PositionId IN ({string.Join(',', PositionIds)})"; }
    }
}
