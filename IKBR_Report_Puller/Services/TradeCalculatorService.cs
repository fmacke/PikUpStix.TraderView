using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Models;

namespace TraderView.Application.Services
{
    public class TradeCalculatorService : ITradeCalculatorService
    {
        private readonly TraderView.Application.Interfaces.Services.IPositionService? _positionService;

        public TradeCalculatorService()
        {
        }

        public TradeCalculatorService(TraderView.Application.Interfaces.Services.IPositionService positionService)
        {
            _positionService = positionService;
        }

        public TradeCalculationResponse CalculatePosition(TradeCalculationRequest request)
        {
            var response = new TradeCalculationResponse();
            var gainLossRatio = request.GainLossRatio / 100m; // e.g., 200% -> 2.0
            var riskPerTrade = request.RiskPerTrade / 100m;   // e.g., 5% -> 0.05
            var maxExposure = request.MaxExposure / 100m;     // e.g., 4% -> 0.04

            // Base capital risked in GBP
            response.LotSizeGbp = request.TradingCapital * riskPerTrade;

            // Scenario 1: Stop Loss calculated by percentage (MaxExposure)
            if (request.StopLossAtInputOverride == null || request.StopLossAtInputOverride == 0)
            {
                response.LotGbp = response.LotSizeGbp;
                response.LotUsd = response.LotGbp * request.ExchangeRate;
                response.LotPercent = 1;
                response.Shares = response.LotUsd / request.BuyPrice;

                // Stop Loss At = BuyPrice - (BuyPrice * MaxExposure)
                response.StopLossAt = request.BuyPrice - (request.BuyPrice * maxExposure);

                // Loss calculations
                response.LossUsd = (request.BuyPrice - response.StopLossAt) * response.Shares;
                response.LossGbp = response.LossUsd / request.ExchangeRate;
                response.LossPercentage = response.LossGbp / request.TradingCapital;
                CalculateProfitMetrics(request, response, gainLossRatio, maxExposure);
            }
            else
            {
                // Scenario 2: Hard Price Stop Loss Override
                response.StopLossAt = Convert.ToDecimal(request.StopLossAtInputOverride);
                var stopLossDistance = request.BuyPrice - response.StopLossAt;

                if (stopLossDistance > 0)
                {
                    var targetRiskUsd = response.LotSizeGbp * request.ExchangeRate * maxExposure;

                    response.Shares = targetRiskUsd / stopLossDistance;
                    response.LotUsd = response.Shares * request.BuyPrice;
                    response.LotGbp = response.LotUsd / request.ExchangeRate;

                    response.LossUsd = response.Shares * stopLossDistance;
                    response.LossGbp = response.LossUsd / request.ExchangeRate;
                    response.LossPercentage = response.LossGbp / request.TradingCapital;
                    response.LotPercent = request.TradingCapital != 0 ? response.LotGbp / request.TradingCapital : 0m;

                    CalculateProfitMetrics(request, response, gainLossRatio, maxExposure);
                }
            }

            return response;
        }

        private static void CalculateProfitMetrics(TradeCalculationRequest request, TradeCalculationResponse response, decimal gainLossRatio, decimal maxExposure)
        {
            // Take Profit Target based on GainLossRatio and MaxExposure
            if (request.TakeProfitAtInputOverride == null || request.TakeProfitAtInputOverride == 0)
            {
                response.TakeProfitAt = gainLossRatio * maxExposure * 100m;
                response.PriceTarget = request.BuyPrice + (request.BuyPrice * (response.TakeProfitAt / 100m));
                response.OverallProfitUsd = (response.PriceTarget - request.BuyPrice) * response.Shares;
                response.OverallProfitGbp = response.OverallProfitUsd / request.ExchangeRate;
            }
            else
            {
                var takeProfitOverride = Convert.ToDecimal(request.TakeProfitAtInputOverride);
                response.PriceTarget = takeProfitOverride;
                var priceGainPercent = (response.PriceTarget - request.BuyPrice) / request.BuyPrice;
                var derivedGainLossRatio = priceGainPercent / maxExposure;
                response.TakeProfitAt = derivedGainLossRatio * maxExposure * 100m;
                response.OverallProfitUsd = (response.PriceTarget - request.BuyPrice) * response.Shares;
                response.OverallProfitGbp = response.OverallProfitUsd / request.ExchangeRate;
            }
        }

        public CompoundPositions CalculateCompoundPositions(TradeCalculationRequest request, TradeCalculationResponse quarterPosition)
        {
            // Define standard breakout continuation step percentage for pyramiding (e.g., +2.5% price advance per add)
            const decimal pyramidStepPercent = 2.5m;

            var buy1 = request.BuyPrice;
            var buy2 = buy1 * (1m + pyramidStepPercent / 100m);
            var buy3 = buy2 * (1m + pyramidStepPercent / 100m);

            var baseRiskPercent = request.RiskPerTrade;
            var takeProfitPercent = quarterPosition.TakeProfitAt;

            decimal? overrideStopLossPrice = null;
            if (quarterPosition != null && quarterPosition.StopLossAt != 0)
            {
                overrideStopLossPrice = quarterPosition.StopLossAt;
            }

            // Tier 3: Pilot Entry (1/4 position block)
            var quarter = ScalePositionTier(request, buy1, baseRiskPercent * 1.0m, takeProfitPercent, overrideStopLossPrice);

            // Tier 2: Half Position Add
            var half = ScalePositionTier(request, buy2, baseRiskPercent * 0.5m, takeProfitPercent, overrideStopLossPrice);

            // Tier 1 / Full Position: Final Add
            var full = ScalePositionTier(request, buy3, baseRiskPercent * 0.5m, takeProfitPercent, overrideStopLossPrice);

            // Aggregate positions cumulatively for weighted cost basis and exposure
            quarter.TotalShares = quarter.Shares;
            quarter.AverageSharePriceUsd = quarter.BuyPriceUsd;
            quarter.TotalSizeGbp = (quarter.Shares * quarter.BuyPriceGbp);

            half.TotalShares = quarter.Shares + half.Shares;
            half.AverageSharePriceUsd = half.TotalShares > 0
                ? ((quarter.Shares * quarter.BuyPriceUsd) + ((half.TotalShares - quarter.Shares) * half.BuyPriceUsd)) / half.TotalShares
                : 0m;
            half.TotalSizeGbp = (half.TotalShares * half.AverageSharePriceUsd) / request.ExchangeRate;

            full.TotalShares = half.TotalShares + full.Shares;
            var incrementalFullShares = full.TotalShares - half.TotalShares;
            full.AverageSharePriceUsd = full.TotalShares > 0
                ? ((quarter.Shares * quarter.BuyPriceUsd) +
                   ((half.TotalShares - quarter.Shares) * half.BuyPriceUsd) +
                   (incrementalFullShares * full.BuyPriceUsd)) / full.TotalShares
                : 0m;
            full.TotalSizeGbp = (full.TotalShares * full.AverageSharePriceUsd) / request.ExchangeRate;

            // Align stop losses with SEPA methodology:
            // Ensure stop loss floors sit at or above the weighted average cost basis to prevent underwater stop-outs.
            half.StopLossAtUsd = half.AverageSharePriceUsd;
            full.StopLossAtUsd = buy2;

            quarter.ProfitTargetUsd = (quarter.TargetSharePriceUsd - quarter.BuyPriceUsd) * quarter.TotalShares;
            quarter.TakeProfitOrPyramidAtUsd = quarter.TargetSharePriceUsd * quarter.TotalShares;
            quarter.WinUsd = (quarter.TargetSharePriceUsd - quarter.AverageSharePriceUsd) * quarter.TotalShares;
            quarter.LossUsd = (quarter.AverageSharePriceUsd - quarter.StopLossAtUsd) * quarter.TotalShares;
            quarter.WinGbp = quarter.WinUsd / request.ExchangeRate;
            quarter.LossGbp = quarter.LossUsd / request.ExchangeRate;
            quarter.WinLossRatioPercentage = quarter.LossUsd != 0 ? 100 * (quarter.WinUsd / quarter.LossUsd) : 0m;

            half.ProfitTargetUsd = (half.TargetSharePriceUsd - half.BuyPriceUsd) * half.TotalShares;
            half.TakeProfitOrPyramidAtUsd = half.TargetSharePriceUsd * half.TotalShares;
            half.WinUsd = (half.TargetSharePriceUsd - half.AverageSharePriceUsd) * half.TotalShares;
            half.WinGbp = half.WinUsd / request.ExchangeRate;
            half.LossUsd = (half.AverageSharePriceUsd - half.StopLossAtUsd) * half.TotalShares;
            half.LossGbp = half.LossUsd / request.ExchangeRate;
            half.WinLossRatioPercentage = half.LossUsd != 0 ? 100 * (half.WinUsd / half.LossUsd) : 0m;

            full.ProfitTargetUsd = (full.TargetSharePriceUsd - full.BuyPriceUsd) * full.TotalShares;
            full.TakeProfitOrPyramidAtUsd = full.TargetSharePriceUsd * full.TotalShares;
            full.WinUsd = (full.TargetSharePriceUsd - full.AverageSharePriceUsd) * full.TotalShares;
            full.LossUsd = (full.AverageSharePriceUsd - full.StopLossAtUsd) * full.TotalShares;
            full.WinGbp = full.WinUsd / request.ExchangeRate;
            full.LossGbp = full.LossUsd / request.ExchangeRate;
            full.WinLossRatioPercentage = full.LossUsd != 0 ? 100 * (full.WinUsd / full.LossUsd) : 0m;

            return new CompoundPositions
            {
                QuarterPosition = quarter,
                HalfPosition = half,
                FullPosition = full
            };
        }

        public async Task<List<CompoundPositions>> GenerateCompoundPositionsReportAsync()
        {
            if (_positionService == null) throw new InvalidOperationException("IPositionService is not available. Ensure service is constructed with IPositionService injected.");

            var openPositions = await _positionService.GetOpenPositionsAsync();
            var results = new List<CompoundPositions>();

            foreach (var pos in openPositions)
            {
                decimal buyPrice = 0m;
                if (pos.LastReportedPrice.HasValue && pos.LastReportedPrice.Value > 0m)
                {
                    buyPrice = Convert.ToDecimal(pos.TradeExecutions.OrderBy(x => x.Id).First().TradePrice);
                }
                else if (pos.TradeExecutions != null && pos.TradeExecutions.Count > 0)
                {
                    var entries = pos.TradeExecutions.Where(te => te.Quantity.HasValue && te.Quantity.Value > 0 && te.TradePrice.HasValue);
                    var totalQty = entries.Sum(e => e.Quantity ?? 0m);
                    if (totalQty > 0m)
                    {
                        var weighted = entries.Sum(e => (e.TradePrice ?? 0m) * (e.Quantity ?? 0m));
                        buyPrice = weighted / totalQty;
                    }
                }

                if (buyPrice <= 0m) continue;

                var exchangeRate = pos.TradeExecutions?.FirstOrDefault(te => te.FxRateToBase.HasValue && te.FxRateToBase.Value > 0m)?.FxRateToBase ?? 1m;

                var request = new TradeCalculationRequest
                {
                    TradeDate = pos.OpenDate,
                    Instrument = pos.Instrument?.InstrumentName ?? string.Empty,
                    BuyPrice = buyPrice,
                    ExchangeRate = exchangeRate
                };

                var quarter = CalculatePosition(request);
                var compound = CalculateCompoundPositions(request, quarter);
                results.Add(compound);
            }

            return results;
        }

        private TradeCalculationCompoundedPosition ScalePositionTier(
            TradeCalculationRequest request,
            decimal buyPriceUsd,
            decimal riskPercentForTier,
            decimal takeProfitPercent,
            decimal? overrideStopLossPrice)
        {
            var positionSizeGbp = request.TradingCapital * (riskPercentForTier / 100m);
            var positionSizeUsd = request.ExchangeRate != 0 ? positionSizeGbp * request.ExchangeRate : 0m;

            decimal stopLossPercent;
            decimal stopLossAtUsd;
            decimal shares;

            if (overrideStopLossPrice.HasValue && overrideStopLossPrice.Value > 0)
            {
                var originalBuy1 = request.BuyPrice;
                var overridePercentage = originalBuy1 > 0 ? (originalBuy1 - overrideStopLossPrice.Value) / originalBuy1 : (request.MaxExposure / 100m);

                stopLossPercent = overridePercentage * 100m;
                stopLossAtUsd = buyPriceUsd * (1m - overridePercentage);
                var stopLossDistance = buyPriceUsd - stopLossAtUsd;

                if (stopLossDistance > 0)
                {
                    var targetRiskUsd = positionSizeUsd * overridePercentage;
                    shares = targetRiskUsd / stopLossDistance;
                }
                else
                {
                    shares = buyPriceUsd > 0 ? positionSizeUsd / buyPriceUsd : 0m;
                }
            }
            else
            {
                stopLossPercent = request.MaxExposure;
                stopLossAtUsd = buyPriceUsd * (1m - (stopLossPercent / 100m));
                var stopLossDistance = buyPriceUsd - stopLossAtUsd;

                if (stopLossDistance > 0)
                {
                    var targetRiskUsd = positionSizeUsd * (request.MaxExposure / 100m);
                    shares = targetRiskUsd / stopLossDistance;
                }
                else
                {
                    shares = buyPriceUsd > 0 ? positionSizeUsd / buyPriceUsd : 0m;
                }
            }

            var positionRiskUsd = shares * (buyPriceUsd - stopLossAtUsd);
            var positionRiskGbp = request.ExchangeRate != 0 ? positionRiskUsd / request.ExchangeRate : 0m;
            var accountRiskPercentage = request.TradingCapital != 0 ? (positionRiskGbp / request.TradingCapital) * 100m : 0m;

            var targetSharePriceUsd = buyPriceUsd * (1m + takeProfitPercent / 100m);
            var profitLossTargetPercentage = targetSharePriceUsd / request.BuyPrice;

            var pos = new TradeCalculationCompoundedPosition
            {
                TradeDate = request.TradeDate,
                Instrument = request.Instrument ?? string.Empty,
                ExchangeRate = request.ExchangeRate,
                TradingCapital = request.TradingCapital,
                RiskPerPositionPercentage = riskPercentForTier,
                StopLossOnPositionPercentage = stopLossPercent,
                PositionSizeUsd = shares * buyPriceUsd,
                PositionSizeGbp = request.ExchangeRate != 0 ? (shares * buyPriceUsd) / request.ExchangeRate : 0m,
                PositionRiskUsd = positionRiskUsd,
                PositionRiskGbp = positionRiskGbp,
                AccountRiskPercentage = accountRiskPercentage,
                BuyPriceUsd = buyPriceUsd,
                BuyPriceGbp = request.ExchangeRate != 0 ? buyPriceUsd / request.ExchangeRate : 0m,
                Shares = shares,
                TotalShares = shares,
                AverageSharePriceUsd = buyPriceUsd,
                StopLossAtUsd = stopLossAtUsd,
                ProfitLossTargetPercentage = request.GainLossRatio,
                ProfitTargetUsd = 0m,
                TakeProfitOrPyramidAtUsd = 0m,
                TargetSharePriceUsd = targetSharePriceUsd,
                TargetSharePricePercentage = profitLossTargetPercentage,
                WinUsd = 0m,
                LossUsd = 0m,
                WinLossRatioPercentage = request.GainLossRatio
            };

            return pos;
        }
    }
}