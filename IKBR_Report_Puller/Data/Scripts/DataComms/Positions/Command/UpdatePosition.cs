using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PikUpStix.TraderView.Data.Scripts.DataComms.Positions.Command
{
    internal class ClosePosition
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
