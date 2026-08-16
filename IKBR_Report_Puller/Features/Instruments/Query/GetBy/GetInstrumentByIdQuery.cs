namespace TraderView.Application.Features.Instruments.Query.GetBy
{
    public class GetInstrumentByIdQuery
    {
        public string Script()
        {
            return @"
                        SELECT p.Id, p.InstrumentId, p.OpenDate, p.Status, p.LastReportedPrice, p.LastReportedPriceUpdated
                        FROM [dbo].[Positions] p WITH (UPDLOCK, ROWLOCK)
                        WHERE p.InstrumentId = @instrumentId
                          AND p.Status = 'Open';";
        }
    }
}
