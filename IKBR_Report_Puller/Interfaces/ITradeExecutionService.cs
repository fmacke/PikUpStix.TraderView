using PikUpStix.TraderView.Domain;

namespace PikUpStix.TraderView.Interfaces
{
    public interface ITradeExecutionService
    {
        /// <summary>
        /// Gets all open positions from the database
        /// </summary>
        /// <returns>List of all open positions</returns>
        Task<List<Position>> GetOpenPositionsAsync();
    }
}
