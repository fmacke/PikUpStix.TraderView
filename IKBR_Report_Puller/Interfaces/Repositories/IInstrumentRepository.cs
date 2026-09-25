using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repository interface for Instrument-related database operations
    /// </summary>
    public interface IInstrumentRepository
    {
        int? GetInstrumentIdByConId(string conid);
        Task<int?> GetInstrumentIdByConIdAsync(string conid);
        Instrument Get(int instrumentId);
        Task<Instrument?> GetAsync(int instrumentId);
        int InsertInstrument(string conid, string symbol, string listingExchange, string currency, string assetCategory, string provider, string dataSource);
        Task<int> InsertInstrumentAsync(string conid, string symbol, string listingExchange, string currency, string assetCategory, string provider, string dataSource);
        Task UpsertInstrumentsAsync(List<TradeExecution> trades, string source);
        Task UpsertInstrumentsAsync(List<TradeConfirm> trades, string source);
        Task<Instrument?> GetByIdAsync(int instrumentId);
    }
}
    