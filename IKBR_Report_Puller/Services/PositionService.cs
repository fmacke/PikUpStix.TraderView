using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Models;
using TraderView.Application.Services;
using TraderView.Domain.Entities;
using TraderView.Application.Specifications;

namespace PikUpStix.TraderView.Services
{
    public class PositionService : IPositionService
    {
        private readonly IPositionRepository _positionRepository;
        private readonly IHistoricalDataService _historicalDataService;

        public PositionService(IPositionRepository positionRepository, IHistoricalDataService historicalDataService)
        {
            _positionRepository = positionRepository ?? throw new ArgumentNullException(nameof(positionRepository));
            _historicalDataService = historicalDataService ?? throw new ArgumentNullException(nameof(historicalDataService));
        }
        async Task IPositionService.ClosePositionAsync(int positionId)
        {
            // Reload the position with tracking enabled to ensure we work with a
            // single tracked instance in the repository's DbContext, avoiding
            // duplicate tracking conflicts from passed-in untracked entities.
            var position = await _positionRepository.GetByIdForUpdateAsync(positionId);
            var historicalData = await _historicalDataService.GetByInstrumentIdAsync(position.InstrumentId);
            position.Instrument.HistoricalData = historicalData.ToList();
            if (position == null)
            {
                throw new InvalidOperationException($"Position with ID {positionId} not found.");
            }
            var closeDate = position.TradeExecutions.Max(te => te.DateTime);
            var closeOutPositionReportData = CalculatePositionMetrics(position);
            position.CloseDate = closeDate;
            position.Status = "Closed";
            position.GrossProfit = closeOutPositionReportData.GrossProfit;
            position.Commission = closeOutPositionReportData.Commission;
            position.NetProfit = closeOutPositionReportData.NetProfit;
            position.MaximumFavourableExcursion = closeOutPositionReportData.MaximumFavourableExcursion;
            position.MaximumAdverseExcursion = closeOutPositionReportData.MaximumAdverseExcursion;
            position.Direction = closeOutPositionReportData.Direction;
            await _positionRepository.UpdateAsync(position);
        }
        private CloseOutPositionReportData CalculatePositionMetrics(Position position)
        {
            if (position == null)
            {
                throw new InvalidOperationException($"Position with ID {position.Id} not found.");
            }
            var grossProfit = new decimal?();
            var commission = new decimal?();
            var netProfit = new decimal?();
            var direction = position.TradeExecutions.MinBy(x => x.DateTime)?.BuySell;
            var maxFavorableExcursion = 0m;
            var maxAdverseExcursion = 0m;

            if (position.TradeExecutions.Count(x => x.TradeId == null) == 0)  // if no TradeId, there is a TradeConfirmation placeholder in TradeExecutions so we cannot calculate MFE/MAE yet
            {
                //var trades = await _repository.GetTradeExecutionsByPositionIdAsync(positionId);
                grossProfit = position.TradeExecutions.Sum(t => t.FifoPnlRealized);
                commission = position.TradeExecutions.Sum(t => t.IbCommission);
                netProfit = grossProfit + commission;
                if (position.TradeExecutions.Count > 0)
                {
                    // Get earliest open date and final close date
                    var earliestOpenDate = position.TradeExecutions.Min(t => t.TradeDate);
                    var finalCloseDate = position.TradeExecutions.Max(t => t.DateTime);
                    var instrumentId = position.TradeExecutions.FirstOrDefault()?.Position?.InstrumentId;

                    // If we have both position and instrument info, fetch historical data for MFE/MAE calculation
                    if (instrumentId.HasValue)
                    {
                        try
                        {
                            if (position.Instrument.HistoricalData.Count > 0)
                            {
                                var histoDataInRange = position.Instrument.HistoricalData
                                    .Where(h => h.Date >= earliestOpenDate && h.Date <= finalCloseDate)
                                    .ToList();
                                var contractUnitMultiplier = 1.0;
                                if(position.Instrument.ContractUnit.HasValue)
                                {
                                    contractUnitMultiplier = Convert.ToDouble(position.Instrument.ContractUnit);
                                }
                                // Get entry price (average price of opening trades)
                                var totalQuantity = position.TradeExecutions.Where(x => x.BuySell == direction).Sum(t => t.Quantity ?? 0);
                                var entryPrice = (decimal)(position.TradeExecutions
                                    .Where(t => t.BuySell == direction)
                                    .Sum(t => (t.TradePrice ?? 0) * (t.Quantity ?? 0)) / totalQuantity);

                                // Calculate investment cost
                                var investmentCost = entryPrice * totalQuantity;

                                // Calculate highest and lowest prices from historical data
                                var highestPrice = (decimal)(histoDataInRange.Max(b => b.HighPrice) / contractUnitMultiplier);
                                var lowestPrice = (decimal)(histoDataInRange.Min(b => b.LowPrice) / contractUnitMultiplier);

                                // Calculate MFE and MAE based on position direction, as percentage of investment cost
                                if (investmentCost != 0)
                                {
                                    if (position.Direction.Equals("BUY", StringComparison.OrdinalIgnoreCase))
                                    {
                                        // For long position: MFE is gain to highest price, MAE is loss to lowest price
                                        var mfeAbsolute = (highestPrice - entryPrice) * totalQuantity;
                                        var maeAbsolute = (lowestPrice - entryPrice) * totalQuantity;

                                        maxFavorableExcursion = (mfeAbsolute / investmentCost) * 100;
                                        maxAdverseExcursion = (maeAbsolute / investmentCost) * 100;
                                    }
                                    else // SELL
                                    {
                                        // For short position: MFE is gain to lowest price, MAE is loss to highest price
                                        var mfeAbsolute = (entryPrice - lowestPrice) * totalQuantity;
                                        var maeAbsolute = (entryPrice - highestPrice) * totalQuantity;

                                        maxFavorableExcursion = (mfeAbsolute / investmentCost) * 100;
                                        maxAdverseExcursion = (maeAbsolute / investmentCost) * 100;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Log error but don't fail - fall back to trade-level MFE/MAE if available
                            Console.WriteLine($"Error calculating MFE/MAE from historical data: {ex.Message}");
                        }
                    }
                }
            }
            return new CloseOutPositionReportData
            {
                GrossProfit = grossProfit,
                Commission = commission,
                NetProfit = netProfit,
                MaximumFavourableExcursion = maxFavorableExcursion,
                MaximumAdverseExcursion = maxAdverseExcursion,
                Direction = direction
            };
        }
        async Task<int> IPositionService.CreatePositionAsync(int instrumentId, DateTime openDate, decimal openPrice, string marketRegime)
        {
            var positionIdTask = await _positionRepository.CreatePositionAsync(instrumentId, openDate, openPrice, marketRegime);
            return positionIdTask;
        }
        async Task<Position?> IPositionService.GetPositionByIdAsync(int positionId)
        {
            return await _positionRepository.GetByIdAsync(positionId);
        }
        async Task<Position?> IPositionService.GetOpenPositionAsync(int instrumentId)
        {
            return await _positionRepository.GetOpenPositionAsync(instrumentId);            
        }
        async Task<IReadOnlyList<Position>?> IPositionService.GetClosedPositionsAsync()
        {
            return await _positionRepository.GetClosedPositionsAsync();
        }
        async Task<IReadOnlyList<Position>> IPositionService.GetOpenPositionsAsync()
        {

            return await Task.Run(() => _positionRepository.GetOpenPositionsAsync());
        }
        async Task<List<Position>> IPositionService.GetAllPositionsAsync(bool loadTradeExecutions)
        {
            return await _positionRepository.GetAllPositionsAsync(loadTradeExecutions);
        }
        async Task IPositionService.UpsertPositionsAsync(IReadOnlyList<Position> positions, string currentMarketRegime)
        {
            if (positions == null || !positions.Any())
            {
                Console.WriteLine("No positions to upsert.");
                return;
            }

            int insertedCount = 0;
            int updatedCount = 0;

            foreach (var position in positions)
            {
                // Ensure instrument exists before upserting position
                if (position.Id == 0)
                {
                    await _positionRepository.CreatePositionAsync(position.InstrumentId, position.OpenDate, position.LastReportedPrice ?? 0m, currentMarketRegime);
                    insertedCount++;
                }
                else
                {
                    await _positionRepository.UpdatePositionAsync(position.Id, DateTime.Now, position.LastReportedPrice ?? 0m, "Open");
                    updatedCount++;
                }
            }
            Console.WriteLine($"Successfully processed {positions.Count} positions: {insertedCount} inserted, {updatedCount} updated.");
        }
    }
}
