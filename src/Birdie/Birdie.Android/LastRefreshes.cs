using Newtonsoft.Json;
using System;

namespace Birdie.Droid
{
    [Serializable]
    public class LastRefreshes
    {
        [JsonProperty("Alarm")]
        public DateTime? Alarm { get; set; }

        [JsonProperty("Worker")]
        public DateTime? Worker { get; set; }

        public LastRefreshes() { }
    }
}
