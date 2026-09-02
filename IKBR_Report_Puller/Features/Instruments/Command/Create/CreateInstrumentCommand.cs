namespace TraderView.Application.Features.Instruments.Command.Create
{
    public class CreateInstrumentCommand : IQueryWithParameters
    {
        public CreateInstrumentCommand(string instrumentName, string provider, string dataName, string dataSource, string format, 
            string frequency, decimal? contractUnit, string contractUnitType, decimal? priceQuotation, decimal? minimumPriceFluctuation, string currency, string listingExchange, int conId)
        {
            InstrumentName = instrumentName;
            Provider = provider;
            DataName = dataName;
            DataSource = dataSource;
            Format = format;
            Frequency = frequency;
            ContractUnit = contractUnit;
            ContractUnitType = contractUnitType;
            PriceQuotation = priceQuotation;
            MinimumPriceFluctuation = minimumPriceFluctuation;
            Currency = currency;
            ListingExchange = listingExchange;
            ConId = conId;
        }
        public string InstrumentName { get; }
        public string Provider { get; }
        public string DataName { get; }
        public string DataSource { get; }
        public string Format { get; }
        public string Frequency { get; }
        public decimal? ContractUnit { get; }
        public string ContractUnitType { get; }
        public decimal? PriceQuotation { get; }
        public decimal? MinimumPriceFluctuation { get; }
        public string Currency { get; }
        public string ListingExchange { get; }
        public int ConId { get; }
        public Dictionary<string, object> Parameters
        {
            get => new Dictionary<string, object>
            {
                { "@instrumentName", InstrumentName },
                { "@provider", Provider },
                { "@dataName", DataName },
                { "@dataSource", DataSource },
                { "@format", Format },
                { "@frequency", Frequency },
                { "@contractUnit", ContractUnit },
                { "@contractUnitType", ContractUnitType },
                { "@priceQuotation", PriceQuotation },
                { "@minimumPriceFluctuation", MinimumPriceFluctuation },
                { "@currency", Currency },
                { "@listingExchange", ListingExchange },
                { "@conId", ConId }
            };
        //    new Dictionary<string, object>
        //            {
        //                { "@instrumentName", symbol ?? "Unknown" },
        //                { "@provider", provider ?? "Unknown" },
        //                { "@dataName", symbol ?? "Unknown" },
        //                { "@dataSource", dataSource ?? "Unknown" },
        //                { "@format", "TradeExecution" },
        //                { "@frequency", "TradeExecution" },
        //                { "@contractUnit", DBNull.Value },
        //                { "@contractUnitType", assetCategory },
        //                { "@priceQuotation", DBNull.Value },
        //                { "@minimumPriceFluctuation", DBNull.Value },
        //                { "@currency", (object)currency ?? DBNull.Value },
        //                { "@listingExchange", (object)listingExchange ?? DBNull.Value },
        //                { "@conId", conid }
        //}
        }
        public string Script
        {
            get => @"
                INSERT INTO dbo.Instruments 
                (InstrumentName, Provider, DataName, DataSource, Format, Frequency, ContractUnit, ContractUnitType, 
                 PriceQuotation, MinimumPriceFluctuation, Currency, ListingExchange, ConId) 
                OUTPUT INSERTED.Id
                VALUES 
                (@instrumentName, @provider, @dataName, @dataSource, @format, @frequency, @contractUnit, @contractUnitType, 
                 @priceQuotation, @minimumPriceFluctuation, @currency, @listingExchange, @conId)";
        }
    }
    
}
