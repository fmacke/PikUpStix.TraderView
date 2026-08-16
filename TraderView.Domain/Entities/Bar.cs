using System.Text.Json.Serialization;

namespace TraderView.Domain.Entities
{
    public class Bar
    {
        [JsonPropertyName("date")]
        [JsonConverter(typeof(FmpDateTimeConverter))]
        public DateTime Date { get; set; }
        [JsonPropertyName("open")]
        public double OpenPrice { get; set; }
        [JsonPropertyName("close")]
        public double ClosePrice { get; set; }
        [JsonPropertyName("low")]
        public double LowPrice { get; set; }
        [JsonPropertyName("high")]
        public double HighPrice { get; set; }
        [JsonPropertyName("volume")]
        public double Volume { get; set; }
        [JsonPropertyName("settle")]
        public double Settle { get; set; }
        [JsonPropertyName("openInterest")]
        public double OpenInterest { get; set; }
        [JsonPropertyName("instrumentId")]
        public int InstrumentId { get; set; }
    }
}
