using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraderView.Application.Features.Positions.Command.Update
{
    public class ClosePositionCommand : IQueryWithParameters
    {
        public ClosePositionCommand(int positionId, DateTime closeDate)
        {
            PositionId = positionId;
            CloseDate = closeDate;
        }

        public int PositionId { get; }
        public DateTime CloseDate { get; }

        public Dictionary<string, object> Parameters
        {
            get => new Dictionary<string, object>
            {
                { "@positionId", PositionId },
                { "@closeDate", CloseDate }
            };
        }   

        public string Script
        {
            get => @"
                UPDATE [dbo].[Positions]
                SET Status = 'Closed', CloseDate = @closeDate
                WHERE Id = @positionId";
        }
    }
}
