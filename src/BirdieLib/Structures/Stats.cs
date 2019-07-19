using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BirdieLib.Structures
{
    [Serializable]
    public class Stats
    {
        [JsonProperty("retweets")]
        public Dictionary<string, int> Retweets { get; set; }

        [JsonProperty("firstRetweet")]
        public DateTime? FirstRetweet { get; set; }

        [JsonProperty("lastRetweet")]
        public DateTime? LastRetweet { get; set; }

        public Stats(Dictionary<string, int> retweets = null, DateTime? firstRetweet = null, DateTime? lastRetweet = null)
        {
            Retweets = retweets ?? new Dictionary<string, int>();
            FirstRetweet = firstRetweet;
            LastRetweet = lastRetweet;
        }
    }
}
