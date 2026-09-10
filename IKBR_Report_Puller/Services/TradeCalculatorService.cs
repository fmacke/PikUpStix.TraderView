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

            // Lot Size £ = TradingCapital * LotSizePercentage (e.g. 100000 * 0.05 = 5000)
            response.LotSizeGbp = request.TradingCapital * request.LotSizePercentage;

            if (request.CalculationMode == "LotSize")
            {
                // Stop Loss set by Lot Size logic
                response.LotGbp = request.Lot * response.LotSizeGbp;
                response.LotUsd = response.LotGbp * request.ExchangeRate;

                // SHARES = (Lot * LotSizeGBP) / BuyPrice (Approximated from excel row 14)
                // Note: Excel formula uses =(B12*E$12)/$B$4 where E12 is USD lot value
                response.Shares = (request.Lot * response.LotUsd) / request.BuyPrice;

                // STOP LOSS AT = BuyPrice - (BuyPrice * MaxExposure)
                response.StopLossAt = request.BuyPrice - (request.BuyPrice * request.MaxExposure);

                // LOSS = (BuyPrice - StopLossAt) * Shares
                response.LossGbp = (request.BuyPrice - response.StopLossAt) * response.Shares;
                response.LossUsd = response.LossGbp / request.ExchangeRate;

                // LOSS % = LossGbp / TradingCapital
                response.LossPercentage = response.LossGbp / request.TradingCapital;

                // TAKE PROFIT AT ratio calculation
                response.TakeProfitAt = request.GainLossRatio * request.MaxExposure;
                response.PriceTarget = request.BuyPrice + (request.BuyPrice * response.TakeProfitAt);

                // OVERALL PROFIT = (PriceTarget - BuyPrice) * Shares
                response.OverallProfitGbp = (response.PriceTarget - request.BuyPrice) * response.Shares;
                response.OverallProfitUsd = response.OverallProfitGbp / request.ExchangeRate;
            }
            else
            {
                // Stop Loss set by Stop Loss Point logic (Right section of excel)
                response.StopLossAt = request.StopLossAtInput;

                // Lot calculation based on stop loss point
                response.Shares = (request.MaxExposure * response.LotSizeGbp * request.ExchangeRate) / (request.BuyPrice - response.StopLossAt);
                response.LotGbp = (response.Shares * request.BuyPrice) / request.ExchangeRate; // Simplified inverse
                response.LotUsd = response.LotGbp * request.ExchangeRate;

                response.LossGbp = response.Shares * (request.BuyPrice - response.StopLossAt);
                response.LossUsd = response.LossGbp / request.ExchangeRate;
                response.LossPercentage = response.LossGbp / request.TradingCapital;

                response.TakeProfitAt = request.GainLossRatio * request.MaxExposure; // Or percentage based
                response.PriceTarget = request.BuyPrice + (request.BuyPrice * (request.GainLossRatio * (request.BuyPrice - response.StopLossAt) / request.BuyPrice));

                response.OverallProfitGbp = response.LossGbp * request.GainLossRatio;
                response.OverallProfitUsd = response.OverallProfitGbp / request.ExchangeRate;
            }

            return response;
        }
    }
}
