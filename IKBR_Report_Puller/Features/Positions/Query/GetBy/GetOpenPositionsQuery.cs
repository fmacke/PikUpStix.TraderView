namespace TraderView.Application.Features.Positions.Query.GetBy
{
    public class GetOpenPositionsQuery
    {
        public string Script()
        {
            return @"SELECT p.Id, p.OpenDate, p.Status, p.InstrumentId, p.LastReportedPrice, p.LastReportedPriceUpdated, " +
                    "i.InstrumentName, i.DataName, i.DataSource, i.Currency, i.ConId, i.ContractUnitType " +
                    "FROM [dbo].[Positions] p " +
                    "INNER JOIN [dbo].[Instruments] i ON p.InstrumentId = i.Id " +
                    "WHERE p.Status = 'Open' " +
                    "ORDER BY p.OpenDate DESC";
        }
    }
}
