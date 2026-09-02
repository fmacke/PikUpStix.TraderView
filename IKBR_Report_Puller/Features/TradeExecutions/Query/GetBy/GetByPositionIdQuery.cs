namespace TraderView.Application.Features.TradeExecutions.Query.GetBy
{
    public class GetByPositionIdQuery : IQueryWithParameters
    {
        private int _positionId;
        public GetByPositionIdQuery(int positionId)
        {
            _positionId = positionId;
        }
        public Dictionary<string, object> Parameters
        {
            get => new Dictionary<string, object>
            {
                { "@PositionId", _positionId }
            };
        }
        public string Script
        {
            get => @"SELECT te.[Id]
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
                  where te.PositionId = @PositionId
                  order by te.[dateTime] desc";
        }
    }
}
