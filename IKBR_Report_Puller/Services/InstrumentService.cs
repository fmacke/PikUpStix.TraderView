using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities;

namespace TraderView.Application.Services
{
    public class InstrumentService : IInstrumentService
    {
        private readonly IInstrumentRepository _repository;

        public InstrumentService(IInstrumentRepository repository)
        {
            _repository = repository;
        }
        public async Task UpsertInstrumentsAsync(List<TradeExecution> trades, string sourceId)
        {
            // Business logic here (validation, transformation, etc.)
            await _repository.UpsertInstrumentsAsync(trades, sourceId).ConfigureAwait(false);
        }

        public async Task UpsertInstrumentsAsync(List<TradeConfirm> trades, string sourceId)
        {
            await _repository.UpsertInstrumentsAsync(trades, sourceId).ConfigureAwait(false);
        }

        public async Task<int?> GetInstrumentIdByConIdAsync(string conId)
        {
            return await _repository.GetInstrumentIdByConIdAsync(conId).ConfigureAwait(false);
        }

        public async Task<Instrument?> GetInstrumentByIdAsync(int instrumentId)
        {
            return await _repository.GetByIdAsync(instrumentId).ConfigureAwait(false);
        }
    }
}
