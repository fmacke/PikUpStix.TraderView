using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{
    public interface IInstrumentService
    {
        Task UpsertInstrumentsAsync(List<TradeExecution> trades, string sourceId);
        Task UpsertInstrumentsAsync(List<TradeConfirm> trades, string sourceId);
        Task<int?> GetInstrumentIdByConIdAsync(string conId);
        Task<Instrument?> GetInstrumentByIdAsync(int instrumentId);
    }
}
