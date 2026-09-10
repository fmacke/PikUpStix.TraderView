using System;
using System.Collections.Generic;
using System.Text;

namespace TraderView.Application.Models
{
    public class TradeCalculationRequest
    {
        public DateTime TradeDate { get; set; } = DateTime.Today;
        public string Instrument { get; set; } = "mrx";
        public decimal ExchangeRate { get; set; } = 1.36m;
        public decimal BuyPrice { get; set; } = 520.0m;
        public decimal TradingCapital { get; set; } = 100000.0m;
        public decimal RiskPerTrade { get; set; } = 5m;
        public decimal MaxExposure { get; set; } = 2.5m;
        public decimal GainLossRatio { get; set; } = 200.0m;
        public decimal? StopLossAtInput { get; set; } = 480.0m; // Used if user wants to specify exact stop loss point (rather thann percentage of positions size)
    }
    public class TradeCalculationResponse
    {
        public decimal LotSizeGbp { get; set; }
        public decimal LotGbp { get; set; }
        public decimal LotUsd { get; set; }
        public decimal LotPercent { get; set; }
        public decimal Shares { get; set; }
        public decimal StopLossAt { get; set; }
        public decimal LossGbp { get; set; }
        public decimal LossUsd { get; set; }
        public decimal LossPercentage { get; set; }
        public decimal TakeProfitAt { get; set; }
        public decimal PriceTarget { get; set; }
        public decimal OverallProfitGbp { get; set; }
        public decimal OverallProfitUsd { get; set; }
    }
}
