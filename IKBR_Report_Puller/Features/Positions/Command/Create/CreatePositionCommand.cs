using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraderView.Application.Features.Positions.Command.Create
{
    public class CreatePositionCommand
    {
        public string Script()
        {
            return @"INSERT INTO [dbo].[Positions] (OpenDate, Status, InstrumentId, LastReportedPrice, LastReportedPriceUpdated)
                            OUTPUT INSERTED.Id
                            VALUES (@openDate, @status, @instrumentId, @lastReportedPrice, @LastReportedPriceUpdated);";
        }
    }
}
