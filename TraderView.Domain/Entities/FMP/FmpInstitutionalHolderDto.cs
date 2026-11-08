using System.Text.Json.Serialization;

namespace TraderView.Domain.Entities.FMP
{
    /// <summary>
    /// Represents institutional holders breakdown from FMP (/v3/institutional-holder/{symbol}).
    /// </summary>
    public class FmpInstitutionalHolderDto
    {
        [JsonPropertyName("holder")]
        public string Holder { get; set; } = string.Empty;

        [JsonPropertyName("shares")]
        public long Shares { get; set; }

        [JsonPropertyName("dateReported")]
        public string DateReported { get; set; } = string.Empty;

        [JsonPropertyName("change")]
        public long Change { get; set; }

        [JsonPropertyName("weight")]
        public decimal? Weight { get; set; }
    }
}