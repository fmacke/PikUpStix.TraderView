using System.Linq.Expressions;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Domain.Entities;

namespace TraderView.Application.Specifications.Instruments
{
    /// <summary>
    /// Specification for retrieving an instrument by its symbol/name
    /// </summary>
    public class InstrumentBySymbolSpecification : BaseSpecification<Instrument>
    {
        public InstrumentBySymbolSpecification(string symbol)
        {
            Criteria = i => i.InstrumentName == symbol;
        }
    }
}
