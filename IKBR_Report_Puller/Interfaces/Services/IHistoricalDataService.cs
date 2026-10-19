using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{
    public interface IHistoricalDataService
    {
        Task<List<Bar>> GetCandlesticksAsync(int instrumentId, DateTime startDate, DateTime endDate);
        Task<int?> GetInstrumentIdBySymbolAsync(string symbol);
        Task<string> DetermineRegimeAsync(DateTime openDate, CancellationToken cancellationToken);
    }
}
