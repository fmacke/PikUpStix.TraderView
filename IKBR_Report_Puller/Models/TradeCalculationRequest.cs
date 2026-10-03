using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.CustomUI;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
    public class TradeCalculationCompoundedPosition
    {
        // Identification & Meta
        public DateTime TradeDate { get; set; }
        public string Instrument { get; set; } = string.Empty;
        public decimal ExchangeRate { get; set; }

        // Capital & Risk Settings
        public decimal TradingCapital { get; set; }
        public decimal RiskPerPositionPercentage { get; set; }
        public decimal StopLossOnPositionPercentage { get; set; }

        // Calculated Position Metrics
        public decimal PositionSizeUsd { get; set; }
        public decimal PositionSizeGbp { get; set; }
        public decimal PositionRiskUsd { get; set; }
        public decimal PositionRiskGbp { get; set; }
        public decimal AccountRiskPercentage { get; set; }

        // Price & Shares
        public decimal BuyPriceUsd { get; set; }
        public decimal BuyPriceGbp { get; set; }
        public decimal Shares { get; set; }
        public decimal TotalShares { get; set; }
        public decimal AverageSharePriceUsd { get; set; }

        // Targets & Stop Loss
        public decimal StopLossAtUsd { get; set; }
        public decimal ProfitLossTargetPercentage { get; set; }
        public decimal ProfitTargetUsd { get; set; }
        public decimal TakeProfitOrPyramidAtUsd { get; set; }
        public decimal TargetSharePriceUsd { get; set; }
        public decimal TargetSharePricePercentage { get; set; }

        // Outcomes & Ratios
        public decimal WinUsd { get; set; }
        public decimal LossUsd { get; set; }
        public decimal WinLossRatioPercentage { get; set; }
    }
    public class CompoundPositions
    {
        public TradeCalculationCompoundedPosition QuarterPosition { get; set; }
        public TradeCalculationCompoundedPosition HalfPosition { get; set; }
        public TradeCalculationCompoundedPosition FullPosition { get; set; }
    }
}
