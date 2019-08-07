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

        public TwitterConfig(bool autoLoad = false, string defaultConsumerKey = null, string defaultConsumerSecret = null)
        {
            if (autoLoad)
            {
                string twitterConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TwitterConfig.json");
                if (!File.Exists(twitterConfigPath))
                {
                    try
                    {
                        File.Copy(Path.Combine(Environment.CurrentDirectory, "TwitterConfig.json"), twitterConfigPath);
                    }
                    catch (Exception)
                    {
                        // If we can't find the resource file, just re-create it.  --Kris
                        File.WriteAllText(twitterConfigPath, JsonConvert.SerializeObject(
                            new TwitterConfig(defaultConsumerKey: "VMWUlZGK9hgnk4iBqELUc7So5", defaultConsumerSecret: "X5gFdMwwDmWB7Dq5FbSVtYETLx5R2GjHYM92x67bRexAFQS3BJ")));
                    }
                }

                try
                {
                    ConfigJSON = File.ReadAllText(twitterConfigPath);
                }
                catch (Exception) { }

                if (string.IsNullOrWhiteSpace(ConfigJSON))
                {
                    ConsumerKey = defaultConsumerKey;
                    ConsumerSecret = defaultConsumerSecret;
                }
                else
                {
                    TwitterConfig twitterConfig = Load();

                    ConsumerKey = twitterConfig.ConsumerKey;
                    ConsumerSecret = twitterConfig.ConsumerSecret;
                    AccessToken = (!twitterConfig.AccessToken.Equals("YourAccessToken") ? twitterConfig.AccessToken : null);
                    AccessTokenSecret = (!twitterConfig.AccessTokenSecret.Equals("YourAccessTokenSecret") ? twitterConfig.AccessTokenSecret : null);
                }
            }
        }

        private TwitterConfig Load()
        {
            return JsonConvert.DeserializeObject<TwitterConfig>(ConfigJSON);
        }

        public void Save()
        {
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TwitterConfig.json"), JsonConvert.SerializeObject(this));
        }
    }
}
