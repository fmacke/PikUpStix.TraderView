using System;

namespace TraderView.Domain.Entities
{
    /// <summary>
    /// PositionOutcome extends Position with calculated outcome metrics such as
    /// Maximum Favourable Excursion (MFE) and Maximum Adverse Excursion (MAE).
    /// </summary>
    public class PositionOutcome : Position
    {
        public decimal MaximumFavourableExcursion { get; set; }
        public decimal MaximumAdverseExcursion { get; set; }
    }
}
