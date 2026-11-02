using DocumentFormat.OpenXml.InkML;
using PikUpStix.TraderView.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Models;
using TraderView.Domain.Entities;

namespace TraderView.Application.Services
{
    public class TradeExecutionService : ITradeExecutionService
    {
        private readonly ITradeExecutionRepository _repository;
        private readonly IPositionService _positionService;
        private readonly IInstrumentService _instrumentService;
        private readonly IHistoricalDataService _historicalDataService;
        public TradeExecutionService(ITradeExecutionRepository repository, IPositionService positionService, IInstrumentService instrumentService, IHistoricalDataService historicalDataService)
        {
            _repository = repository;
            _positionService = positionService;
            _instrumentService = instrumentService;
            _historicalDataService = historicalDataService;
        }
        async Task<List<TradeExecution>> ITradeExecutionService.GetTradeExecutionsAsync()
        {
            return await _repository.GetTradeExecutionsAsync();
        }
        async Task<List<TradeExecution>> ITradeExecutionService.GetTradeExecutionsByPositionIdAsync(int positionId)
        {
            return await _repository.GetTradeExecutionsByPositionIdAsync(positionId);
        }
        async Task<List<(DateTime TradeDate, decimal Quantity, string OpenCloseIndicator)>> ITradeExecutionService.GetTradeExecutionsByConIdAndAccountAsync(long? conid, string accountId)
        {
            return await _repository.GetTradeExecutionsByConIdAndAccountAsync(conid, accountId);
        }
        async Task<TradeSummary?> ITradeExecutionService.GetTradeSummaryByPositionIdAsync(int positionId)
        {
            return await _repository.GetTradeSummaryByPositionIdAsync(positionId);
        }
        async Task ITradeExecutionService.UpsertTradeExecutionsAsync(IReadOnlyList<TradeExecution> trades)
        {
            if (trades == null || !trades.Any())
            {
                Console.WriteLine("No trades to insert.");
                return;
            }

            foreach (var trade in trades)
            {
                string ibExecID = trade.IbExecId;
                if (string.IsNullOrEmpty(ibExecID))
                {
                    continue;
                }

                var executionExists = await _repository.TradeExecutionExistsAsync(ibExecID);

                if (!executionExists)
                {
                    try
                    {
                        //trade.Position.InstrumentId = Convert.ToInt32(await _instrumentService.GetInstrumentIdByConIdAsync(trade.Conid));
                        var openPosition = await _positionService.GetOpenPositionAsync(trade.Position.InstrumentId);
                        if (openPosition != null)
                        {
                            // Link by FK only. Do NOT assign the navigation property from an entity
                            // that may be tracked by another context or service instance — that
                            // results in "another instance with the same key value is already being tracked".
                            trade.PositionId = openPosition.Id;
                            trade.Position = null;
                        }
                        else
                        {
                            // No open position — create a fresh Position instance so EF will add it
                            // together with the TradeExecution. Avoid reusing any Position instance
                            // that might have been created or tracked elsewhere.
                            var newPosition = new Position
                            {
                                InstrumentId = trade.Position?.InstrumentId ?? 0,
                                OpenDate = trade.TradeDate,
                                Status = "Open",
                                LastReportedPrice = trade.Position?.LastReportedPrice,
                                LastReportedPriceUpdated = trade.Position?.LastReportedPriceUpdated
                            };

                            trade.Position = newPosition;
                            trade.PositionId = null; // let EF set FK when saving
                        }
                        var insertTrade = await _repository.AddAsync(trade);
                        trade.Id = insertTrade.Id;
                        var totalQuantity = await _repository.GetTotalQuantityForPositionAsync(trade.PositionId ?? 0);
                        if (totalQuantity == 0)
                        {
                            var closeOutPositionReportData = await CalculatePositionMetrics(Convert.ToInt32(trade.PositionId));
                            await _positionService.ClosePositionAsync(trade.PositionId ?? 0, trade.DateTime, closeOutPositionReportData);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error inserting trade with ibExecID {ibExecID}: {ex.Message}");
                    }
                }
                else
                {
                    var tradeExecInDb = await _repository.GetTradeExecutionByExecIDAsync(ibExecID);
                    if (tradeExecInDb != null && !tradeExecInDb.TransactionId.HasValue)
                    {
                        // Entry was made by TradeConfirmation so will be missing key details. Update the record with the new trade execution details.
                        trade.Id = tradeExecInDb.Id; // Ensure we have the correct Id for the update
                        trade.PositionId = tradeExecInDb.PositionId; // Preserve the existing PositionId

                        // Map incoming values into the tracked entity to avoid multiple tracked
                        // instances with the same key. Copy scalar values from the incoming
                        // trade into the tracked tradeExecInDb and persist.
                        try
                        {
                            // Copy scalar properties from the incoming trade into the tracked
                            // entity instance so we don't attach a second instance to the
                            // DbContext. Exclude Id and PositionId which we want to keep
                            // as-is (PositionId preserved above).
                            var props = typeof(TradeExecution)
                                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .Where(p => p.CanRead && p.CanWrite && p.Name != nameof(TradeExecution.Id) && p.Name != nameof(TradeExecution.PositionId));

                            foreach (var prop in props)
                            {
                                // Heuristic: skip navigation/reference-type properties
                                // except string which is a scalar type we want to copy.
                                if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                                    continue;

                                var val = prop.GetValue(trade);
                                prop.SetValue(tradeExecInDb, val);
                            }

                            await _repository.UpdateAsync(tradeExecInDb);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error updating trade execution (Id: {tradeExecInDb.Id}): {ex.Message}");
                        }
                    }
                }
            }
        }
        private async Task<CloseOutPositionReportData> CalculatePositionMetrics(int positionId)
        {

            var trades = await _repository.GetTradeExecutionsByPositionIdAsync(positionId);
            var grossProfit = trades.Sum(t => t.FifoPnlRealized);
            var commission = trades.Sum(t => t.IbCommission);
            var netProfit = grossProfit - commission;
            var direction = trades.FirstOrDefault()?.BuySell ?? "BUY";

            // Calculate MFE and MAE using historical price data
            var maxFavorableExcursion = 0m;
            var maxAdverseExcursion = 0m;

            if (trades.Count > 0)
            {
                // Get earliest open date and final close date
                var earliestOpenDate = trades.Min(t => t.TradeDate);
                var finalCloseDate = trades.Max(t => t.DateTime);
                var instrumentId = trades.FirstOrDefault()?.Position?.InstrumentId;

                // If we have both position and instrument info, fetch historical data for MFE/MAE calculation
                if (instrumentId.HasValue)
                {
                    try
                    {
                        // Get candlestick data for the position period
                        var historicalBars = await _historicalDataService.GetCandlesticksAsync(
                            instrumentId.Value,
                            earliestOpenDate,
                            finalCloseDate);

                        if (historicalBars.Count > 0)
                        {
                            // Get entry price (average price of opening trades)
                            var totalQuantity = trades.Sum(t => t.Quantity ?? 0);
                            var entryPrice = (decimal)(trades
                                .Where(t => t.BuySell == "BUY" || t.BuySell == "SELL")
                                .Sum(t => (t.TradePrice ?? 0) * (t.Quantity ?? 0)) / totalQuantity);

                            // Calculate investment cost
                            var investmentCost = entryPrice * totalQuantity;

                            // Calculate highest and lowest prices from historical data
                            var highestPrice = (decimal)historicalBars.Max(b => b.HighPrice);
                            var lowestPrice = (decimal)historicalBars.Min(b => b.LowPrice);

                            // Calculate MFE and MAE based on position direction, as percentage of investment cost
                            if (investmentCost != 0)
                            {
                                if (direction.Equals("BUY", StringComparison.OrdinalIgnoreCase))
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
        async Task ITradeExecutionService.UpsertTradeConfirmationsAsync(IReadOnlyList<TradeConfirm> tradeConfirms, string currentMarketRegime)
        {
            if (tradeConfirms == null || !tradeConfirms.Any())
            {
                Console.WriteLine("No trade confirmations to process.");
                return;
            }

            foreach (var tradeConfirm in tradeConfirms)
            {
                if (string.IsNullOrEmpty(tradeConfirm.IbExecID))
                {
                    Console.WriteLine("TradeExecution confirmation missing execID. Skipping.");
                    continue;
                }

                if (!await _repository.TradeExecutionExistsAsync(tradeConfirm.IbExecID))
                {
                    var instrumentId = await _instrumentService.GetInstrumentIdByConIdAsync(tradeConfirm.Conid);

                    if (instrumentId.HasValue)
                    {
                        Position? existingPosition = null;

                        // Check for open position for the trade's symbol and instrument
                        existingPosition = await _positionService.GetOpenPositionAsync(instrumentId.Value);

                        if (existingPosition != null)
                        {
                            var openDirection = existingPosition.TradeExecutions.MinBy(x => x.Id).BuySell;
                            tradeConfirm.PositionId = existingPosition.Id;
                            tradeConfirm.OpenCloseIndicator = "O";
                            if (tradeConfirm.BuySell != openDirection)
                            {
                                tradeConfirm.OpenCloseIndicator = "C";
                                if (existingPosition?.TradeExecutions.Sum(x => x.Quantity) + tradeConfirm.Quantity == 0)
                                {
                                    var closeOutPositionReportData = await CalculatePositionMetrics(existingPosition.Id);
                                    await _positionService.ClosePositionAsync(existingPosition.Id, tradeConfirm.DateTime, closeOutPositionReportData);
                                }
                            }
                        }
                        else
                        {
                            tradeConfirm.OpenCloseIndicator = "O";
                            tradeConfirm.PositionId = await _positionService.CreatePositionAsync(instrumentId.Value, tradeConfirm.TradeDate, tradeConfirm.TradePrice, currentMarketRegime);
                        }
                        tradeConfirm.Id = await _repository.CreateTradeConfirmationAsync(tradeConfirm);
                    }
                    else
                    {
                        Console.WriteLine($"Instrument not found for symbol {tradeConfirm.Symbol} with Conid {tradeConfirm.Conid}. Skipping trade confirmation.");
                    }
                }
            }
            Console.WriteLine("Successfully processed today's trade confirmations.");
        }
    }
}

