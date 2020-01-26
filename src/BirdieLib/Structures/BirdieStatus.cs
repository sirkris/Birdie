using Newtonsoft.Json;
using System;
using System.IO;

namespace BirdieLib
{
    [Serializable]
    public class BirdieStatus
    {
        [JsonProperty("Active")]
        public bool Active { get; set; }

        [JsonProperty("ActiveSince")]
        public DateTime? ActiveSince { get; set; }

        [JsonProperty("LastActive")]
        public DateTime? LastActive { get; set; }

        private readonly string StatusJSON;
        private string StatusFilename
        {
            get
            {
                return "BirdieStatus.json";
            }
            set { }
        }

        public BirdieStatus(bool autoLoad = false)
        {
            if (autoLoad)
            {
                string birdieStatusPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), StatusFilename);
                if (!File.Exists(birdieStatusPath))
                {
                    try
                    {
                        File.Copy(Path.Combine(Environment.CurrentDirectory, StatusFilename), birdieStatusPath);
                    }
                    catch (Exception)
                    {
                        // If we can't find the resource file, just re-create it.  --Kris
                        File.WriteAllText(birdieStatusPath, JsonConvert.SerializeObject(new BirdieStatus()));
                    }
                }

                try
                {
                    StatusJSON = File.ReadAllText(birdieStatusPath);
                }
                catch (Exception) { }

                if (!string.IsNullOrWhiteSpace(StatusJSON))
                {
                    BirdieStatus birdieStatus = Load();

                    Active = birdieStatus.Active;
                    ActiveSince = birdieStatus.ActiveSince;
                    LastActive = birdieStatus.LastActive;
                }
            }
        }

        public void Clear()
        {
            Active = false;
            ActiveSince = null;
            LastActive = null;
        }

        private BirdieStatus Load()
        {
            return JsonConvert.DeserializeObject<BirdieStatus>(StatusJSON);
        }

        public void Save()
        {
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), StatusFilename), JsonConvert.SerializeObject(this));
        }
    }
}
