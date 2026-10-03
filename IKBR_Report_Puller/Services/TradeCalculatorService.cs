using System;
using System.Collections.Generic;
using System.Text;
using TraderView.Application.Interfaces.Services;
using TraderView.Application.Models;

namespace TraderView.Application.Services
{
    public class TradeCalculatorService : ITradeCalculatorService
    {
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
        public CompoundPositions CalculateCompoundPositions(TradeCalculationResponse quarterPosition)
        {
            var halfPosition = ScalePosition(quarterPosition, 2m);
            var fullPosition = ScalePosition(quarterPosition, 4m);

            var compoundPositions = new CompoundPositions
            {
                // QuarterPosition needs to be a TradeCalculationCompoundedPosition. Create from quarterPosition with multiplier=1
                QuarterPosition = ScalePosition(quarterPosition, 1m),
                HalfPosition = halfPosition,
                FullPosition = fullPosition
            };

            return compoundPositions;
        }

        private TradeCalculationCompoundedPosition ScalePosition(TradeCalculationResponse basePosition, decimal multiplier)
        {
            // Map available fields from TradeCalculationResponse into the new TradeCalculationCompoundedPosition
            // Some fields (like trading capital or instrument) are not present on TradeCalculationResponse
            // and therefore are left as defaults or derived where possible.
            var pos = new TradeCalculationCompoundedPosition
            {
                TradeDate = DateTime.Today,
                Instrument = string.Empty,
                ExchangeRate = 0m,

                // Risk / capital related - derive what we can
                TradingCapital = 0m,
                RiskPerPositionPercentage = basePosition.LotPercent * 100m,
                StopLossOnPositionPercentage = basePosition.LossPercentage * 100m,

                // Scaled position sizes
                PositionSizeUsd = basePosition.LotUsd * multiplier,
                PositionSizeGbp = basePosition.LotGbp * multiplier,
                PositionRiskUsd = basePosition.LossUsd * multiplier,
                PositionRiskGbp = basePosition.LossGbp * multiplier,
                AccountRiskPercentage = basePosition.LossPercentage * multiplier * 100m,

                // Price and shares
                BuyPriceUsd = 0m,
                BuyPriceGbp = 0m,
                Shares = basePosition.Shares * multiplier,
                TotalShares = basePosition.Shares * multiplier,
                AverageSharePriceUsd = 0m,

                // Targets & stop loss
                StopLossAtUsd = basePosition.StopLossAt,
                ProfitLossTargetPercentage = basePosition.TakeProfitAt,
                ProfitTargetUsd = basePosition.OverallProfitUsd * multiplier,
                TakeProfitOrPyramidAtUsd = basePosition.OverallProfitUsd * multiplier,
                TargetSharePriceUsd = basePosition.PriceTarget,
                TargetSharePricePercentage = basePosition.TakeProfitAt,

                // Outcomes
                WinUsd = basePosition.OverallProfitUsd * multiplier,
                LossUsd = basePosition.LossUsd * multiplier,
                WinLossRatioPercentage = basePosition.TakeProfitAt
            };

            return pos;
        }
    }
    
}
