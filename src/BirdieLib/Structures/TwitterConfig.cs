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

        private readonly string ConfigPath;

        public TwitterConfig(string configPath)
        {
            if (string.IsNullOrEmpty(configPath))
            {
                configPath = Environment.CurrentDirectory;
            }
            ConfigPath = configPath;

            TwitterConfig twitterConfig = Load();

            ConsumerKey = twitterConfig.ConsumerKey;
            ConsumerSecret = twitterConfig.ConsumerSecret;
            AccessToken = twitterConfig.AccessToken;
            AccessTokenSecret = twitterConfig.AccessTokenSecret;
        }

        public TwitterConfig() { }

        private TwitterConfig Load()
        {
            return JsonConvert.DeserializeObject<TwitterConfig>(File.ReadAllText(Path.Combine(ConfigPath, "TwitterConfig.json")));
        }

        public void Save()
        {
            File.WriteAllText(Path.Combine(ConfigPath, "TwitterConfig.json"), JsonConvert.SerializeObject(this));
        }
    }
}
