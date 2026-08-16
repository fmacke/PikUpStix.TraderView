using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraderView.Application.Features.Positions.Command.Update
{
    public class ClosePositionCommand
    {
        public string Script()
        {
            return @"
                UPDATE [dbo].[Positions]
                SET Status = 'Closed', CloseDate = @closeDate
                WHERE Id = @positionId";
        }
    }
}
