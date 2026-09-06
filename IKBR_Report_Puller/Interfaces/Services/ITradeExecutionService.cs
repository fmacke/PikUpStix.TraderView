using TraderView.Domain.Entities;

namespace TraderView.Application.Interfaces.Services
{
    public interface ITradeExecutionService
    {
        Task<List<TradeExecution>> GetTradeExecutions();
    }
}
