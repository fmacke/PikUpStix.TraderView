namespace TraderView.Application.Features.Positions.Query.GetBy
{
    public class GetAllPositionsQuery
    {
        public string Script()
        {
            return @"SELECT p.Id, p.OpenDate, p.CloseDate, p.Status, p.InstrumentId, p.LastReportedPrice, p.LastReportedPriceUpdated, " +
                    "i.InstrumentName, i.DataName,i.Currency, i.ConId, i.ContractUnitType  " +
                    "FROM [dbo].[Positions] p " +
                    "INNER JOIN [dbo].[Instruments] i ON p.InstrumentId = i.Id " +
                    "ORDER BY p.OpenDate DESC";
        }
    }
}
