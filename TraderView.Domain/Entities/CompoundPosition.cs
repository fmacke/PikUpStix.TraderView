using System;

namespace TraderView.Domain.Entities
{
    public class CompoundPosition
    {
        public int Id { get; set; }

        public int PositionCalculatorId { get; set; }
        public virtual PositionCalculator? PositionCalculator { get; set; }

        public DateTime TradeDate { get; set; }
        public string Instrument { get; set; } = string.Empty;
        public decimal ExchangeRate { get; set; }

        public decimal TradingCapital { get; set; }
        public decimal RiskPerPositionPercentage { get; set; }
        public decimal StopLossOnPositionPercentage { get; set; }

        public decimal PositionSizeUsd { get; set; }
        public decimal PositionSizeGbp { get; set; }
        public decimal PositionRiskUsd { get; set; }
        public decimal PositionRiskGbp { get; set; }
        public decimal AccountRiskPercentage { get; set; }

        public decimal BuyPriceUsd { get; set; }
        public decimal BuyPriceGbp { get; set; }
        public decimal Shares { get; set; }
        public decimal TotalShares { get; set; }
        public decimal AverageSharePriceUsd { get; set; }

        public decimal StopLossAtUsd { get; set; }
        public decimal ProfitLossTargetPercentage { get; set; }
        public decimal ProfitTargetUsd { get; set; }
        public decimal TakeProfitOrPyramidAtUsd { get; set; }
        public decimal TargetSharePriceUsd { get; set; }
        public decimal TargetSharePricePercentage { get; set; }

        public decimal WinUsd { get; set; }
        public decimal LossUsd { get; set; }
        public decimal WinLossRatioPercentage { get; set; }
    }
}
