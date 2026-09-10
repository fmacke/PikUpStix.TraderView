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
        public decimal LotSizePercentage { get; set; } = 0.05m; // e.g. 0.05 (5%)
        public decimal Lot { get; set; } = 1.0m;
        public decimal MaxExposure { get; set; } = 0.025m;
        public decimal GainLossRatio { get; set; } = 2.0m;

        // Mode: "LotSize" or "StopLossPoint"
        public string CalculationMode { get; set; } = "LotSize";
        public decimal StopLossAtInput { get; set; } = 480.0m; // Used if StopLossPoint mode
    }
    public class TradeCalculationResponse
    {
        public decimal LotSizeGbp { get; set; }
        public decimal LotGbp { get; set; }
        public decimal LotUsd { get; set; }
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
