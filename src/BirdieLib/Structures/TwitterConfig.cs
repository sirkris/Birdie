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

        private readonly string ConfigJSON;

        public TwitterConfig(bool autoLoad = false)
        {
            if (autoLoad)
            {
                using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("BirdieLib.TwitterConfig.json"))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        ConfigJSON = reader.ReadToEnd();
                    }
                }

                if (string.IsNullOrWhiteSpace(ConfigJSON))
                {
                    throw new Exception("Unable to load TwitterConfig.json.");
                }

                TwitterConfig twitterConfig = Load();

                ConsumerKey = twitterConfig.ConsumerKey;
                ConsumerSecret = twitterConfig.ConsumerSecret;
                AccessToken = twitterConfig.AccessToken;
                AccessTokenSecret = twitterConfig.AccessTokenSecret;
            }
        }

        private TwitterConfig Load()
        {
            return JsonConvert.DeserializeObject<TwitterConfig>(ConfigJSON);
        }

        public void Save()
        {
            using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("BirdieLib.TwitterConfig.json"))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(JsonConvert.SerializeObject(this));
                }
            }
        }
    }
}
