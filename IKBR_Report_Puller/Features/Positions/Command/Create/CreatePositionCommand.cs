using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraderView.Application.Features.Positions.Command.Create
{
    public class CreatePositionCommand : IQueryWithParameters
    {
        public CreatePositionCommand(int instrumentId, DateTime openDate, decimal lastReportedPrice, string openCloseIndicator, bool isCurrencyTransaction)
        {
            InstrumentId = instrumentId;
            LastReportedPrice = lastReportedPrice;
            OpenCloseIndicator = openCloseIndicator;
            OpenDate = openDate;
            Status = isCurrencyTransaction ? "Closed" : "Open";
        }

        public int InstrumentId { get; }
        public decimal LastReportedPrice { get; }
        public string OpenCloseIndicator { get; }
        public string Status { get; } = "Open";
        public DateTime LastReportedPriceUpdated { get; } = DateTime.UtcNow;
        public DateTime OpenDate { get; }
    

        public string Script
        {
            get => @"INSERT INTO [dbo].[Positions] (OpenDate, Status, InstrumentId, LastReportedPrice, LastReportedPriceUpdated)
                            OUTPUT INSERTED.Id
                            VALUES (@openDate, @status, @instrumentId, @lastReportedPrice, @LastReportedPriceUpdated);";
        }

        public Dictionary<string, object> Parameters
        {
            get => new Dictionary<string, object>
            {
                { "@openDate", OpenDate },
                { "@status", Status },
                { "@instrumentId", InstrumentId },
                { "@lastReportedPrice", LastReportedPrice },
                { "@LastReportedPriceUpdated", LastReportedPriceUpdated }
            };
        }
    }
}
