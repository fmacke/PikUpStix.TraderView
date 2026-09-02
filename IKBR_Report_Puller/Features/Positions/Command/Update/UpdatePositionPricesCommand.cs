namespace TraderView.Application.Features.Positions.Command.Update
{
    public class UpdatePositionPricesCommand : IQueryWithParameters
    {
        public UpdatePositionPricesCommand(int positionId, decimal lastReportedPrice, DateTime lastReportedPriceUpdated)
        {
            PositionId = positionId;
            LastReportedPrice = lastReportedPrice;
            LastReportedPriceUpdated = lastReportedPriceUpdated;
        }

        public int PositionId { get; }
        public decimal LastReportedPrice { get; }
        public DateTime LastReportedPriceUpdated { get; }

        public string Script
        {
            get => @"UPDATE [dbo].[Positions]
                    SET LastReportedPrice = @lastReportedPrice, LastReportedPriceUpdated = @lastReportedPriceUpdated
                    WHERE Id = @positionId";
        }

        public Dictionary<string, object> Parameters
        {
            get => new Dictionary<string, object>
            {
                { "@positionId", PositionId },
                { "@lastReportedPrice", LastReportedPrice },
                { "@lastReportedPriceUpdated", LastReportedPriceUpdated }
            };
        }
    }
}
