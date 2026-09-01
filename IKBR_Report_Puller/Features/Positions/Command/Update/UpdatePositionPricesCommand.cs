namespace TraderView.Application.Features.Positions.Command.Update
{
    public class UpdatePositionPricesCommand
    {
        public string Script()
        {
            return @"UPDATE [dbo].[Positions]
                    SET LastReportedPrice = @lastReportedPrice, LastReportedPriceUpdated = @lastReportedPriceUpdated
                    WHERE Id = @positionId";
        }
    }
}
