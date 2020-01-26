using Newtonsoft.Json;
using System;

namespace BirdieLib
{
    [Serializable]
    public class Stats
    {
        [JsonProperty("retweets")]
        public int Retweets { get; set; }

        [JsonProperty("firstRetweet")]
        public DateTime? FirstRetweet { get; set; }

        [JsonProperty("lastRetweet")]
        public DateTime? LastRetweet { get; set; }

        public Stats(int retweets = 0, DateTime? firstRetweet = null, DateTime? lastRetweet = null)
        {
            Retweets = retweets;
            FirstRetweet = firstRetweet;
            LastRetweet = lastRetweet;
        }
    }
}
