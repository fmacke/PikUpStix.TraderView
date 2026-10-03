using System.ComponentModel.DataAnnotations;

namespace traderview.Server.DTOs
{
    public class PositionCalculatorDto
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
        public IEnumerable<CompoundPositionDto>? CompoundPositions { get; set; }
    }

    public class PositionCalculatorCreateDto
    {
        [Required]
        public int PositionId { get; set; }

        [Required]
        public DateTime OrderSetupDate { get; set; }

        [Required, StringLength(100)]
        public string Symbol { get; set; } = null!;

        [Required, StringLength(50)]
        public string CurrencyPair { get; set; } = null!;

        [Range(0, double.MaxValue)]
        public decimal ExchangeRate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ProposedPurchasePrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TradingCapital { get; set; }

        [Range(0, double.MaxValue)]
        public decimal RiskPerPosition { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MaxExposureOnPosition { get; set; }

        public decimal GainLossRatioPercent { get; set; }

        public decimal? StopLossAtOverride { get; set; }

        [Required]
        public int StrategyId { get; set; }

        public decimal LotSizeAccountCurrency { get; set; }
        public decimal LotSizeStockCurrency { get; set; }
        public decimal LotSizePercent { get; set; }
        public decimal ShareQuantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal StopLossAt { get; set; }

        public decimal LossCurrency { get; set; }
        public decimal LossPercent { get; set; }
        public decimal PriceTarget { get; set; }
        public decimal TakeProfitAtPercent { get; set; }
        public decimal OverallProfitAccountCurrency { get; set; }
        public decimal OverallProfitStockCurrency { get; set; }
        public string? Comment { get; set; }
        public IEnumerable<CompoundPositionDto>? CompoundPositions { get; set; }
    }

    public class CompoundPositionDto
    {
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

    public class PositionCalculatorUpdateDto : PositionCalculatorCreateDto
    {
        [Required]
        public int Id { get; set; }
    }
}
