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
        private string ConfigFilename
        {
            get
            {
                return "TwitterConfig.json";
            }
            set { }
        }

        private const string DEFAULT_CONSUMER_KEY = "VMWUlZGK9hgnk4iBqELUc7So5";
        private const string DEFAULT_CONSUMER_SECRET = "X5gFdMwwDmWB7Dq5FbSVtYETLx5R2GjHYM92x67bRexAFQS3BJ";

        public TwitterConfig(bool autoLoad = false)
        {
            if (autoLoad)
            {
                string twitterConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ConfigFilename);
                if (!File.Exists(twitterConfigPath))
                {
                    try
                    {
                        File.Copy(Path.Combine(Environment.CurrentDirectory, ConfigFilename), twitterConfigPath);
                    }
                    catch (Exception)
                    {
                        // If we can't find the resource file, just re-create it.  --Kris
                        File.WriteAllText(twitterConfigPath, JsonConvert.SerializeObject(new TwitterConfig()));
                    }
                }

                try
                {
                    ConfigJSON = File.ReadAllText(twitterConfigPath);
                }
                catch (Exception) { }

                if (string.IsNullOrWhiteSpace(ConfigJSON))
                {
                    ConsumerKey = DEFAULT_CONSUMER_KEY;
                    ConsumerSecret = DEFAULT_CONSUMER_SECRET;
                }
                else
                {
                    TwitterConfig twitterConfig = Load();

                    ConsumerKey = (twitterConfig.ConsumerKey != null && !twitterConfig.ConsumerKey.Equals("YourConsumerKey")
                        ? twitterConfig.ConsumerKey : DEFAULT_CONSUMER_KEY);
                    ConsumerSecret = (twitterConfig.ConsumerSecret != null && !twitterConfig.ConsumerSecret.Equals("YourConsumerSecret")
                        ? twitterConfig.ConsumerSecret : DEFAULT_CONSUMER_SECRET);
                    AccessToken = (twitterConfig.AccessToken != null && !twitterConfig.AccessToken.Equals("YourAccessToken") ? twitterConfig.AccessToken : null);
                    AccessTokenSecret = (twitterConfig.AccessTokenSecret != null && !twitterConfig.AccessTokenSecret.Equals("YourAccessTokenSecret") ? twitterConfig.AccessTokenSecret : null);
                }
            }
        }

        public void Clear()
        {
            ConsumerKey = DEFAULT_CONSUMER_KEY;
            ConsumerSecret = DEFAULT_CONSUMER_SECRET;
            AccessToken = null;
            AccessTokenSecret = null;
        }

        private TwitterConfig Load()
        {
            return JsonConvert.DeserializeObject<TwitterConfig>(ConfigJSON);
        }

        public void Save()
        {
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ConfigFilename), JsonConvert.SerializeObject(this));
        }
    }
}
