using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

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
