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
        public IEnumerable<TwitterUser> TwitterUsers { get; set; }

        [JsonProperty("ranks")]
        public Dictionary<int, string> Ranks { get; set; }

        [JsonProperty("stats")]
        public Stats Stats { get; set; }

        public Target(string name, IEnumerable<TwitterUser> twitterUsers, Dictionary<int, string> ranks, Stats stats = null)
        {
            Name = name;
            TwitterUsers = twitterUsers;
            Ranks = ranks;
            Stats = stats ?? new Stats();
        }
    }
}
