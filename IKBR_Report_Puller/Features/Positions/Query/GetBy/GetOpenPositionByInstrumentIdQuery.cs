namespace TraderView.Application.Features.Positions.Query.GetBy
{
    public class GetOpenPositionByInstrumentIdQuery : IQueryWithParameters
    {
        public GetOpenPositionByInstrumentIdQuery(int instrumentId)
        {
            Parameters = new Dictionary<string, object>
                        {
                            { "@instrumentId", instrumentId }
                        };
            Script = @"
                        SELECT p.Id, p.InstrumentId, p.OpenDate, p.Status, p.LastReportedPrice, p.LastReportedPriceUpdated
                        FROM [dbo].[Positions] p WITH (UPDLOCK, ROWLOCK)
                        WHERE p.InstrumentId = @instrumentId
                          AND p.Status = 'Open';";
        }

        public Dictionary<string, object> Parameters { get; set; }
        public string Script { get; set; }
    }
}
