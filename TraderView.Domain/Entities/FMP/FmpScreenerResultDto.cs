using System.Text.Json.Serialization;

namespace TraderView.Domain.Entities.FMP
{
    public class FmpScreenerResultDto
    {
        public string Symbol { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Changes { get; set; }
        public decimal Volume { get; set; }
        public decimal MarketCap { get; set; }

        [JsonPropertyName("exchangeShortName")] 
        public string Exchange { get; set; } = string.Empty;
        public string Sector { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
    }
}
