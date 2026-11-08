using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TraderView.Domain.Entities.FMP
{
    /// <summary>
    /// Represents the company profile response from FMP (/v3/profile).
    /// </summary>
    public class FmpCompanyProfile
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("beta")]
        public decimal? Beta { get; set; }

        [JsonPropertyName("volAvg")]
        public long VolAvg { get; set; }

        [JsonPropertyName("mktCap")]
        public long MarketCap { get; set; }

        [JsonPropertyName("lastDiv")]
        public decimal? LastDiv { get; set; }

        [JsonPropertyName("range")]
        public string Range { get; set; } = string.Empty;

        [JsonPropertyName("changes")]
        public decimal? Changes { get; set; }

        [JsonPropertyName("companyName")]
        public string CompanyName { get; set; } = string.Empty;

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("cik")]
        public string Cik { get; set; } = string.Empty;

        [JsonPropertyName("isin")]
        public string Isin { get; set; } = string.Empty;

        [JsonPropertyName("cusip")]
        public string Cusip { get; set; } = string.Empty;

        [JsonPropertyName("exchange")]
        public string Exchange { get; set; } = string.Empty;

        [JsonPropertyName("exchangeShortName")]
        public string ExchangeShortName { get; set; } = string.Empty;

        [JsonPropertyName("industry")]
        public string Industry { get; set; } = string.Empty;

        [JsonPropertyName("website")]
        public string Website { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("ceo")]
        public string Ceo { get; set; } = string.Empty;

        [JsonPropertyName("sector")]
        public string Sector { get; set; } = string.Empty;

        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;

        [JsonPropertyName("fullTimeEmployees")]
        public string FullTimeEmployees { get; set; } = string.Empty;

        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;

        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;

        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;

        [JsonPropertyName("zip")]
        public string Zip { get; set; } = string.Empty;

        [JsonPropertyName("image")]
        public string Image { get; set; } = string.Empty;

        [JsonPropertyName("ipoDate")]
        public string IpoDate { get; set; } = string.Empty;

        [JsonPropertyName("isEtf")]
        public bool IsEtf { get; set; }

        [JsonPropertyName("isActivelyTrading")]
        public bool IsActivelyTrading { get; set; }
    }
}