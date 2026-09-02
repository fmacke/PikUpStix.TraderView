using DocumentFormat.OpenXml.Math;
using System;
using System.Collections.Generic;
using System.Text;

namespace TraderView.Application.Features.Positions.Query.Get
{
    public class PositionRecordExists : IQueryWithParameters
    {
        public PositionRecordExists(int positionId) { PositionId = positionId; }
        public int PositionId { get; }
        public Dictionary<string, object> Parameters { get => new Dictionary<string, object> { { "@positionId", PositionId } }; }
        public string Script { get => $"SELECT COUNT(*) FROM dbo.Positions WHERE Id = @positionId"; }
    }
}
