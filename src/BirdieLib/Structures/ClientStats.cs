using BirdieLib.Converters;
using Newtonsoft.Json;
using System;

namespace BirdieLib.Structures
{
    [Serializable]
    public class ClientStats
    {
        [JsonProperty("TotalUsers")]
        public int? TotalUsers { get; set; }

        [JsonProperty("ActiveUsers")]
        public int? ActiveUsers { get; set; }

        [JsonProperty("MyLastRetweet")]
        [JsonConverter(typeof(TimestampConvert))]
        public DateTime? MyLastRetweet { get; set; }

        [JsonProperty("MyTotalRetweets")]
        public int MyTotalRetweets { get; set; }

        [JsonProperty("TotalRetweets")]
        public int? TotalRetweets { get; set; }
    }
}
