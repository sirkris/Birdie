using System;
using System.Collections.Generic;

namespace BirdieLib
{
    [Serializable]
    public class Target
    {
        public string Name { get; set; }
        public IEnumerable<TwitterUser> TwitterUsers { get; set; }
        public Dictionary<int, string> Ranks { get; set; }
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
