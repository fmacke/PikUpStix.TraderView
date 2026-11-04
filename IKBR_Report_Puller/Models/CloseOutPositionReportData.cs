using System;
using System.Collections.Generic;
using System.Text;

namespace TraderView.Application.Models
{
    public class CloseOutPositionReportData
    {        
        public decimal? GrossProfit { get; set; }
        public decimal? Commission { get; set; }
        public decimal? NetProfit { get; set; }
        public decimal? MaximumFavourableExcursion { get; set; }
        public decimal? MaximumAdverseExcursion { get; set; }
        public string Direction { get; set; } = "BUY";
    }
}
