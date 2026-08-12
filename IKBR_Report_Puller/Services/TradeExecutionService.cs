using DocumentFormat.OpenXml.Drawing.Charts;
using PikUpStix.TraderView.Data.Repositories;
using PikUpStix.TraderView.Domain;
using PikUpStix.TraderView.Interfaces;

namespace PikUpStix.TraderView.Services
{
    public class TradeExecutionService : ITradeExecutionService
    {
        private readonly ITradeExecutionRepository _tradeExecutionRepository;
        private readonly IPositionRepository _positionRepository;

        public TradeExecutionService(ITradeExecutionRepository tradeExecutionRepository, IPositionRepository positionRepository)
        {
            _tradeExecutionRepository = tradeExecutionRepository;
            _positionRepository = positionRepository;
        }

        async Task<List<Position>> ITradeExecutionService.GetOpenPositionsAsync()
        {
            return await Task.Run(() => GetOpenPositions());
        }

        private async Task<List<Position>>? GetOpenPositions()
        {
            var positions = _tradeExecutionRepository.GetOpenPositions();
            //foreach (var position in positions)
            //{
            //    position.Quantity = position.TradeExecutions.Sum(te => te.Quantity);
            //    position.PositionValue = position.Quantity * position.LastReportedPrice;
            //    //position.CostBasisPrice = position.Quantity / position.;
            //}
            return positions;
        }

        /// <summary>
        /// Gets an open position by symbol and instrument ID asynchronously
        /// </summary>
        async Task<Position?> ITradeExecutionService.GetOpenPositionAsync(string symbol, int instrumentId)
        {
            return await Task.Run(() => _positionRepository.GetOpenPosition(symbol, instrumentId));
        }

        /// <summary>
        /// Creates a new position asynchronously
        /// </summary>
        async Task<int> ITradeExecutionService.CreatePositionAsync(int instrumentId, string symbol, DateTime openDate, decimal openPrice)
        {
            return await Task.Run(() => _positionRepository.CreatePosition(instrumentId, symbol, openDate, openPrice));
        }
    }
}
