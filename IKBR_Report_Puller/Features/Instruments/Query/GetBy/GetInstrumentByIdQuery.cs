namespace TraderView.Application.Features.Instruments.Query.GetBy
{
    public class GetInstrumentByIdQuery : IQueryWithParameters
    {
        private readonly int _id;
        public GetInstrumentByIdQuery(int id)
        {
            _id = id;
        }

        public Dictionary<string, object> Parameters
        {
            get => new Dictionary<string, object>();
        }
        public string Script { get => $@"SELECT [Id]
              ,[InstrumentName]
              ,[Provider]
              ,[DataName]
              ,[DataSource]
              ,[Format]
              ,[Frequency]
              ,[ContractUnit]
              ,[ContractUnitType]
              ,[PriceQuotation]
              ,[MinimumPriceFluctuation]
              ,[Currency]
              ,[ListingExchange]
              ,[ConId]
          FROM [TradingBE].[dbo].[Instruments]
          WHERE Id = {_id}"; }
    };
}


