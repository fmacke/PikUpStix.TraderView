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

                    response.LotPercent = (response.Shares * request.BuyPrice) / (response.LotSizeGbp * request.ExchangeRate) ;
                    response.TakeProfitAt = gainLossRatio * maxExposure * 100; // Or percentage based
                    response.PriceTarget = request.BuyPrice + (request.BuyPrice * (gainLossRatio * (request.BuyPrice - response.StopLossAt) / request.BuyPrice));

                    response.OverallProfitGbp = response.LossGbp * gainLossRatio;
                    response.OverallProfitUsd = response.OverallProfitGbp * request.ExchangeRate;
                }
            }

            return response;
        }
    }
}
