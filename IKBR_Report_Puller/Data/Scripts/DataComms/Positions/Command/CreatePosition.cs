using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PikUpStix.TraderView.Data.Scripts.DataComms.Positions.Command
{
    internal class CreatePosition
    {
        public string Script()
        {
            return @"INSERT INTO [dbo].[Positions] (OpenDate, Status, InstrumentId, LastReportedPrice, LastReportedPriceUpdated)
                            OUTPUT INSERTED.Id
                            VALUES (@openDate, @status, @instrumentId, @lastReportedPrice, @LastReportedPriceUpdated);";
        }
    }
}
