using Newtonsoft.Json;
using System;
using System.IO;

namespace BirdieLib
{
    [Serializable]
    public class TwitterConfig
    {
        [JsonProperty("ConsumerKey")]
        public string ConsumerKey { get; set; }

        [JsonProperty("ConsumerSecret")]
        public string ConsumerSecret { get; set; }

        [JsonProperty("AccessToken")]
        public string AccessToken { get; set; }

        [JsonProperty("AccessTokenSecret")]
        public string AccessTokenSecret { get; set; }

        public TwitterConfig(string configPath = null)
        {
            TwitterConfig twitterConfig = Load(configPath);

            ConsumerKey = twitterConfig.ConsumerKey;
            ConsumerSecret = twitterConfig.ConsumerSecret;
            AccessToken = twitterConfig.AccessToken;
            AccessTokenSecret = twitterConfig.AccessTokenSecret;
        }

        private TwitterConfig Load(string configPath = null)
        {
            if (string.IsNullOrEmpty(configPath))
            {
                configPath = Environment.CurrentDirectory;
            }

            return JsonConvert.DeserializeObject<TwitterConfig>(File.ReadAllText(Path.Combine(configPath, "TwitterConfig.json")));
        }
    }
}
