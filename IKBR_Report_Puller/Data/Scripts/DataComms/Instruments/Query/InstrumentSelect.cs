namespace PikUpStix.TraderView.Data.Scripts.DataComms.Instruments.Query
{
    public class InstrumentGetByConId
    {
        public string Script()
        {
            return "SELECT Id FROM dbo.Instruments WHERE ConId = @conid";
        }
    }
    public class InstrumentGetById
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
