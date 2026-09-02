using System;
using System.Collections.Generic;
using System.Text;

namespace TraderView.Application.Features.Instruments.Query.GetBy
{
    public class GetInstrumentBySymbolAndProviderQuery : IQueryWithParameters
    {
        public GetInstrumentBySymbolAndProviderQuery(string symbol, string provider)
        {
            Symbol = symbol;
            Provider = provider;
        }
        public string Symbol { get; }
        public string Provider { get; }
        public Dictionary<string, object> Parameters
        {
            get => new Dictionary<string, object>
            {
                { "@Symbol", Symbol },
                { "@Provider", Provider }
            };
        }
        public string Script
        {
            get => @"SELECT Id FROM [dbo].[Instruments] WHERE DataName = @Symbol AND DataSource = @Provider";
        }
    }
}
