using TraderView.Application.Interfaces.Persistence;
using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for Instrument-related database operations
    /// </summary>
    public interface IInstrumentRepository : IRepository<Instrument>
    {
        Task<int?> GetInstrumentIdByConIdAsync(string conid);
        Task UpsertInstrumentsAsync(List<TradeExecution> trades, string source);
        Task UpsertInstrumentsAsync(List<TradeConfirm> trades, string source);
    }
}
    