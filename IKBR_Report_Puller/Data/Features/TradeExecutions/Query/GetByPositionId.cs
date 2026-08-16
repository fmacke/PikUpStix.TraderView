namespace PikUpStix.TraderView.Data.Features.TradeExecutions.Query
{
    internal class GetTradeExecutionsByPositionId
    {
        public string Script()
        {
            return @"SELECT 
                        te.Id, p.InstrumentId, te.symbol, te.tradeID, te.dateTime, te.tradeDate, 
                        te.quantity, te.tradePrice, te.buySell, te.fifoPnlRealized, te.ibCommission,
                        te.openCloseIndicator
                    FROM TradeExecutions te
                    INNER JOIN [dbo].[Positions] p ON te.PositionID = p.Id 
                    WHERE PositionID = @PositionId
                    ORDER BY tradeDate, dateTime";
        }
    }
}
