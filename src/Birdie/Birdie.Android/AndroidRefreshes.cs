using Newtonsoft.Json;
using System;
using Environment = System.Environment;
using System.IO;

namespace Birdie.Droid
{
    public static class AndroidRefreshes
    {
        public static DateTime? Alarm
        {
            get
            {
                return LastRefreshes.Alarm;
            }
            set
            {
                lastRefreshes = null;
                LastRefreshes.Alarm = value;

                SaveLastRefreshes();
            }
        }

        public static DateTime? Worker
        {
            get
            {
                return LastRefreshes.Worker;
            }
            set
            {
                lastRefreshes = null;
                LastRefreshes.Worker = value;

                SaveLastRefreshes();
            }
        }

        private static LastRefreshes LastRefreshes
        {
            get
            {
                if (lastRefreshes == null || !LastRefreshesLastRefreshed.HasValue || LastRefreshesLastRefreshed.Value.AddSeconds(5) < DateTime.Now)
                {
                    LastRefreshes = JsonConvert.DeserializeObject<LastRefreshes>(LoadLastRefreshes());
                }

                return lastRefreshes;
            }
            set
            {
                lastRefreshes = value;
                LastRefreshesLastRefreshed = DateTime.Now;
            }
        }
        private static LastRefreshes lastRefreshes;
        private static DateTime? LastRefreshesLastRefreshed { get; set; }

        private static string JSONFilename
        {
            get
            {
                return "LastRefreshes.json";
            }
            set { }
        }

        private static string LoadLastRefreshes()
        {
            string jsonPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), JSONFilename);
            if (!File.Exists(jsonPath))
            {
                try
                {
                    File.Copy(Path.Combine(Environment.CurrentDirectory, JSONFilename), jsonPath);
                }
                catch (Exception)
                {
                    // If we can't find the resource file, just re-create it.  --Kris
                    File.WriteAllText(jsonPath, JsonConvert.SerializeObject(new LastRefreshes()));
                }
            }

            return File.ReadAllText(jsonPath);
        }

        private static void SaveLastRefreshes()
        {
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), JSONFilename), JsonConvert.SerializeObject(LastRefreshes));
        }
    }
}
