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
            var gainLossRatio = request.GainLossRatio / 100; //200%
            var riskPerTrade = request.RiskPerTrade / 100;   //5%
            var maxExposure = request.MaxExposure / 100;     //2.5%

            // RiskPerTrade Size £ = TradingCapital * LotSizePercentage (e.g. 100000 * 5/100 = 5000)
            response.LotSizeGbp = request.TradingCapital * riskPerTrade;

            if (request.StopLossAtInput == null || request.StopLossAtInput == 0)
            {

                // Stop Loss set by RiskPerTrade Size logic
                response.LotGbp = response.LotSizeGbp;
                response.LotUsd = response.LotGbp * request.ExchangeRate;
                response.LotPercent = 1;
                // SHARES = (RiskPerTrade * LotSizeGBP) / BuyPrice (Approximated from excel row 14)
                // Note: Excel formula uses =(B12*E$12)/$B$4 where E12 is USD riskPerTrade value
                response.Shares = (response.LotUsd) / request.BuyPrice;

                // STOP LOSS AT = BuyPrice - (BuyPrice * MaxExposure)
                response.StopLossAt = request.BuyPrice - (request.BuyPrice * (maxExposure));

                // LOSS = (BuyPrice - StopLossAt) * Shares
                response.LossUsd = (request.BuyPrice - response.StopLossAt) * response.Shares;
                response.LossGbp = response.LossUsd / request.ExchangeRate;

                // LOSS % = LossGbp / TradingCapital
                response.LossPercentage = response.LossGbp / request.TradingCapital;

                // TAKE PROFIT AT ratio calculation
                response.TakeProfitAt = gainLossRatio * maxExposure * 100;
                response.PriceTarget = request.BuyPrice + (request.BuyPrice * (response.TakeProfitAt / 100));

                // OVERALL PROFIT = (PriceTarget - BuyPrice) * Shares
                response.OverallProfitUsd = (response.PriceTarget - request.BuyPrice) * response.Shares;
                response.OverallProfitGbp = response.OverallProfitUsd / request.ExchangeRate;
            }
            else
            {
                if (request.StopLossAtInput != null && request.StopLossAtInput != 0)  // sometime user inputs empty figure here so no calc should be made
                {
                    // Stop Loss set by Stop Loss Point logic (Right section of excel)
                    response.StopLossAt = Convert.ToDecimal(request.StopLossAtInput);

                    // RiskPerTrade calculation based on stop loss point
                    response.Shares = (maxExposure * response.LotSizeGbp * request.ExchangeRate) / (request.BuyPrice - response.StopLossAt);
                    response.LotGbp = (response.Shares * request.BuyPrice) / request.ExchangeRate; // Simplified inverse
                    response.LotUsd = response.LotGbp * request.ExchangeRate;

                    response.LossUsd = response.Shares * (request.BuyPrice - response.StopLossAt);
                    response.LossGbp = response.LossUsd / request.ExchangeRate;
                    response.LossPercentage = response.LossUsd / request.TradingCapital;

                    response.LotPercent = (response.Shares * request.BuyPrice) / (response.LotSizeGbp * request.ExchangeRate);
                    response.TakeProfitAt = gainLossRatio * maxExposure * 100; // Or percentage based
                    response.PriceTarget = request.BuyPrice + (request.BuyPrice * (gainLossRatio * (request.BuyPrice - response.StopLossAt) / request.BuyPrice));

                    response.OverallProfitGbp = response.LossGbp * gainLossRatio;
                    response.OverallProfitUsd = response.OverallProfitGbp * request.ExchangeRate;
                }
            }

            return response;
        }
        public CompoundPositions CalculateCompoundPositions(TradeCalculationRequest request, TradeCalculationResponse quarterPosition)
        {
            var takeProfitPercent = quarterPosition.TakeProfitAt;

            var buy1 = request.BuyPrice;
            var target1 = buy1 * (1 + takeProfitPercent / 100m);
            var buy2 = target1;
            var target2 = buy2 * (1 + takeProfitPercent / 100m);
            var buy3 = target2;

            var baseRiskPercent = request.RiskPerTrade;

            var quarter = ScalePositionTier(request, buy1, baseRiskPercent * 1m, takeProfitPercent);
            var half = ScalePositionTier(request, buy2, baseRiskPercent * 2m, takeProfitPercent);
            var full = ScalePositionTier(request, buy3, baseRiskPercent * 4m, takeProfitPercent);

            quarter.TotalShares = quarter.Shares;
            quarter.AverageSharePriceUsd = quarter.BuyPriceUsd;

    
            half.TotalShares = half.Shares;
            half.Shares = half.Shares - quarter.Shares;
            half.AverageSharePriceUsd = half.TotalShares > 0
                ? ((quarter.Shares * quarter.BuyPriceUsd) + (half.Shares * half.BuyPriceUsd)) / half.TotalShares
                : 0m;

            full.TotalShares = full.Shares;
            full.Shares = full.Shares - quarter.Shares - half.Shares;
            full.AverageSharePriceUsd = full.TotalShares > 0
                ? ((quarter.Shares * quarter.BuyPriceUsd) + (half.Shares * half.BuyPriceUsd) + (full.Shares * full.BuyPriceUsd)) / full.TotalShares
                : 0m;

            quarter.ProfitTargetUsd = (quarter.TargetSharePriceUsd - quarter.BuyPriceUsd) * quarter.TotalShares;
            quarter.TakeProfitOrPyramidAtUsd = quarter.TargetSharePriceUsd * quarter.TotalShares;
            quarter.WinUsd = (quarter.TargetSharePriceUsd - quarter.AverageSharePriceUsd) * quarter.TotalShares;
            quarter.LossUsd = (quarter.AverageSharePriceUsd - quarter.StopLossAtUsd) * quarter.TotalShares;

            half.ProfitTargetUsd = (half.TargetSharePriceUsd - half.BuyPriceUsd) * half.TotalShares;
            half.TakeProfitOrPyramidAtUsd = half.TargetSharePriceUsd * half.TotalShares;
            half.WinUsd = (half.TargetSharePriceUsd - half.AverageSharePriceUsd) * half.TotalShares;
            half.LossUsd = (half.AverageSharePriceUsd - half.StopLossAtUsd) * half.TotalShares;

            full.ProfitTargetUsd = (full.TargetSharePriceUsd - full.BuyPriceUsd) * full.TotalShares;
            full.TakeProfitOrPyramidAtUsd = full.TargetSharePriceUsd * full.TotalShares;
            full.WinUsd = (full.TargetSharePriceUsd - full.AverageSharePriceUsd) * full.TotalShares;
            full.LossUsd = (full.AverageSharePriceUsd - full.StopLossAtUsd) * full.TotalShares;

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
                // Determine a buy price: prefer LastReportedPrice, otherwise compute weighted average of entry trades
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

                if (buyPrice <= 0m) continue; // skip positions without a valid price

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


        private TradeCalculationCompoundedPosition ScalePositionTier(TradeCalculationRequest request, decimal buyPriceUsd, decimal riskPercentForTier, decimal takeProfitPercent)
        {
            var positionSizeUsd = request.TradingCapital * (riskPercentForTier / 100m);
            var positionSizeGbp = request.ExchangeRate != 0 ? positionSizeUsd / request.ExchangeRate : 0m;

            var shares = buyPriceUsd > 0 ? positionSizeUsd / buyPriceUsd : 0m;

            var stopLossPercent = request.MaxExposure / 100m;
            var positionRiskUsd = positionSizeUsd * stopLossPercent;
            var positionRiskGbp = request.ExchangeRate != 0 ? positionRiskUsd / request.ExchangeRate : 0m;
            var accountRiskPercentage = request.TradingCapital != 0 ? (positionRiskGbp / request.TradingCapital) * 100m : 0m;

            var stopLossAtUsd = buyPriceUsd * (1 - stopLossPercent);
            var targetSharePriceUsd = buyPriceUsd * (1 + takeProfitPercent / 100m);

            var pos = new TradeCalculationCompoundedPosition
            {
                TradeDate = request.TradeDate,
                Instrument = request.Instrument ?? string.Empty,
                ExchangeRate = request.ExchangeRate,
                TradingCapital = request.TradingCapital,
                RiskPerPositionPercentage = riskPercentForTier,
                StopLossOnPositionPercentage = request.MaxExposure,
                PositionSizeUsd = positionSizeUsd,
                PositionSizeGbp = positionSizeGbp,
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
                TargetSharePricePercentage = takeProfitPercent,
                WinUsd = 0m,
                LossUsd = 0m,
                WinLossRatioPercentage = request.GainLossRatio
            };

            return pos;
        }
    }
    
}
