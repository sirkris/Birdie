using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BirdieLib
{
    [Serializable]
    public class Target
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("twitterUsers")]
        public IEnumerable<string> TwitterUsers { get; set; }

        [JsonProperty("ranks")]
        public Dictionary<int, string> Ranks { get; set; }

        [JsonProperty("stats")]
        public Stats Stats { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        public Target(string name, IEnumerable<string> twitterUsers, Dictionary<int, string> ranks, bool enabled = true, Stats stats = null)
        {
            Name = name;
            TwitterUsers = twitterUsers;
            Ranks = ranks;
            Enabled = enabled;
            Stats = stats ?? new Stats();
        }
    }
}
