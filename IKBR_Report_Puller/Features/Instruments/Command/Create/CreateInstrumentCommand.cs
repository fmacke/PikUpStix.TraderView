namespace TraderView.Application.Features.Instruments.Command.Create
{
    public class CreateInstrumentCommand
    {
        public string Script()
        {
            return @"
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
