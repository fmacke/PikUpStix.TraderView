using TraderView.Domain.Entities;

namespace TraderView.Application.Specifications.Instruments
{
    /// <summary>
    /// Specification to select an Instrument by its ConId
    /// </summary>
    public class GetInstrumentByConIdSpecification : BaseSpecification<Instrument>
    {
        /// <summary>
        /// Create a new specification that matches the provided ConId
        /// </summary>
        public GetInstrumentByConIdSpecification(string conId)
        {
            Criteria = i => i.ConId == conId;
        }
    }
}
