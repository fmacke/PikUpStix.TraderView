using System;

namespace TraderView.Domain.Entities
{
    public class PositionCalculator
    {
        public int Id { get; set; }

        public int PositionId { get; set; }

        public DateTime OrderSetupDate { get; set; }

        public string Symbol { get; set; } = null!;

        public string CurrencyPair { get; set; } = null!;

        public decimal ExchangeRate { get; set; }

        public decimal ProposedPurchasePrice { get; set; }

        public decimal TradingCapital { get; set; }

        public decimal RiskPerPosition { get; set; }

        public decimal MaxExposureOnPosition { get; set; }

        public decimal GainLossRatioPercent { get; set; }

        public decimal? StopLossAtOverride { get; set; }

        public int StrategyId { get; set; }

        public decimal LotSizeAccountCurrency { get; set; }

        public decimal LotSizeStockCurrency { get; set; }

        public decimal LotSizePercent { get; set; }

        public decimal ShareQuantity { get; set; }

        public decimal StopLossAt { get; set; }

        public decimal LossCurrency { get; set; }

        public decimal LossPercent { get; set; }

        public decimal PriceTarget { get; set; }

        public decimal TakeProfitAtPercent { get; set; }

        public decimal OverallProfitAccountCurrency { get; set; }

        public decimal OverallProfitStockCurrency { get; set; }
        public string? Comment { get; set; }
        public virtual Strategy? Strategy { get; set; }
        public virtual ICollection<CompoundPosition>? CompoundPositions { get; set; }
    }
}
