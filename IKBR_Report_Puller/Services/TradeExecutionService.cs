using DocumentFormat.OpenXml.InkML;
using PikUpStix.TraderView.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities;

namespace TraderView.Application.Services
{
    public class TradeExecutionService : ITradeExecutionService
    {
        private readonly ITradeExecutionRepository _repository;
        private readonly IPositionService _positionService;
        private readonly IInstrumentService _instrumentService;
        public TradeExecutionService(ITradeExecutionRepository repository, IPositionService positionService, IInstrumentService instrumentService)
        {
            _repository = repository;
            _positionService = positionService;
            _instrumentService = instrumentService;
        }
        async Task<List<TradeExecution>> ITradeExecutionService.GetTradeExecutionsAsync()
        {
            return await _repository.GetTradeExecutionsAsync();
        }
        async Task<List<TradeExecution>> ITradeExecutionService.GetTradeExecutionsByPositionIdAsync(int positionId)
        {
            return await _repository.GetTradeExecutionsByPositionIdAsync(positionId);
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
                            await _positionService.ClosePositionAsync(trade.PositionId ?? 0, trade.DateTime);
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
                                    await _positionService.ClosePositionAsync(existingPosition.Id, tradeConfirm.TradeDate);
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

