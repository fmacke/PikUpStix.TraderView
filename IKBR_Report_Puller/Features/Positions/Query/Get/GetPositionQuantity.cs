using DocumentFormat.OpenXml.Math;
using System;
using System.Collections.Generic;
using System.Text;

namespace TraderView.Application.Features.Positions.Query.Get
{
    public class GetPositionQuantityByPositionId : IQueryWithParameters
    {
        public GetPositionQuantityByPositionId(int positionId)
        {
            PositionId = positionId;
        }

        public int PositionId { get; }
        public string Script
        {
            get => $@"SELECT ISNULL(SUM(quantity), 0) as TotalQuantity
                            FROM [dbo].[TradeExecutions]
                            WHERE PositionID = { PositionId }";
        }
        public Dictionary<string, object> Parameters
        {
            get => new Dictionary<string, object>();
        }
    }
}
