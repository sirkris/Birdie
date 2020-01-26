using BirdieLib.EventArgs;
using BirdieLib.Structures;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Tweetinvi;
using Tweetinvi.Models;

namespace BirdieLib
{
    public class BirdieLib
    {
        private DateTime? LastCheck;
        private Thread ControlThread;
        public Dictionary<string, IEnumerable<ITweet>> Tweets;

        public TwitterCredentials TwitterCredentials;
        public IAuthenticationContext AuthenticationContext;

        private Request Request { get; set; }
        public ClientStats ClientStats
        {
            get
            {
                if (clientStats == null || !ClientStatsLastRefresh.HasValue || ClientStatsLastRefresh.Value.AddMinutes(1) < DateTime.Now)
                {
                    GetStats();
                }

                return clientStats;
            }
            private set
            {
                clientStats = value;
                ClientStatsLastRefresh = DateTime.Now;
            }
        }
        private ClientStats clientStats;
        private DateTime? ClientStatsLastRefresh;

        public event EventHandler<RetweetEventArgs> RetweetsUpdate;
        public event EventHandler<StatusUpdateEventArgs> StatusUpdate;

        public Dictionary<string, Target> Targets;

        // Structure:  Original Tweet URL => Username
        public Dictionary<string, string> RetweetHistory;

        public TwitterConfig TwitterConfig;

        private readonly Dictionary<string, string> TwitterUserFullnames;
        public Dictionary<string, TwitterUser> TwitterUsers;

        // In test mode, everything functions normally except no retweets are actually sent out.  --Kris
        private readonly bool TestMode;

        // In script mode, the control loop runs once, then termination exits.  --Kris
        public bool ScriptMode { get; set; }

        public BirdieStatus BirdieStatus
        {
            get
            {
                if (birdieStatus == null)
                {
                    birdieStatus = new BirdieStatus(true);
                }

                return birdieStatus;
            }
            set
            {
                birdieStatus = value;
            }
        }
        private BirdieStatus birdieStatus;

        public bool Active
        {
            get
            {
                return BirdieStatus.Active;
            }
            private set
            {
                BirdieStatus.Active = value;
                BirdieStatus.ActiveSince = (value ? (!BirdieStatus.ActiveSince.HasValue ? (DateTime?)DateTime.Now : BirdieStatus.ActiveSince) : null);
                BirdieStatus.LastActive = DateTime.Now;

                BirdieStatus.Save();

                // Fire an event indicating that the Active status has changed.  --Kris
                StatusUpdateEventArgs args = new StatusUpdateEventArgs
                {
                    Active = BirdieStatus.Active
                };
                StatusUpdate?.Invoke(this, args);
            }
        }

        public BirdieLib(bool testMode = false, bool scriptMode = false, bool autoStart = false)
        {
            Active = BirdieStatus.Active;

            TwitterUsers = new Dictionary<string, TwitterUser>();
            TestMode = testMode;
            ScriptMode = scriptMode;

            Request = new Request();

            // Load config, stats, and retweet history.  --Kris
            string targetsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "targets.json");
            if (File.Exists(targetsPath))
            {
                try
                {
                    Targets = JsonConvert.DeserializeObject<Dictionary<string, Target>>(File.ReadAllText(targetsPath));
                }
                catch (Exception) { }
            }

            string retweetHistoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "retweetHistory.json");
            if (File.Exists(retweetHistoryPath))
            {
                try
                {
                    RetweetHistory = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(retweetHistoryPath));
                }
                catch (Exception) { }
            }

            TwitterConfig = new TwitterConfig(true);
            if (!string.IsNullOrWhiteSpace(TwitterConfig.AccessToken) && !string.IsNullOrWhiteSpace(TwitterConfig.AccessTokenSecret))
            {
                LoadTwitterCredentials();
            }

            TweetinviConfig.CurrentThreadSettings.TweetMode = TweetMode.Extended;
            TweetinviConfig.CurrentThreadSettings.InitialiseFrom(TweetinviConfig.ApplicationSettings);

            // Only Bernie's tweets are monitored by default.  --Kris
            if (Targets == null || Targets.Count == 0)
            {
                Targets = new Dictionary<string, Target>
                {
                    {
                        "Bernie Sanders", new Target("Bernie Sanders", new List<string>
                                          {
                                              "BernieSanders",
                                              "SenSanders"
                                          },
                                          new Dictionary<int, string>  // The score value is the number of retweets.  --Kris
                                          {
                                              { 0, "Poser" },
                                              { 1, "Rookie Berner" },
                                              { 50, "Volunteer" },
                                              { 250, "Aspiring Revolutionary" },
                                              { 500, "Birdie Bro" },
                                              { 1000, "Berner" },
                                              { 2500, "Fictional Chair-Thrower" },
                                              { 5000, "Social Media Soldier" },
                                              { 7000, "Berning Man" },
                                              { 8000, "Diabolical Socalist" },
                                              { 10000, "Berner-Elite" },
                                              { 15000, "Revolutionary Legend" }
                                          })
                    },
                    {
                        "Tulsi Gabbard", new Target("Tulsi Gabbard", new List<string> { "TulsiGabbard" },
                                         new Dictionary<int, string>
                                         {
                                              { 0, "Poser" },
                                              { 1, "Tulsi Gabbard Supporter" }
                                         }, false)
                    },
                    {
                        "Jill Stein", new Target("Jill Stein", new List<string> { "DrJillStein" },
                                         new Dictionary<int, string>
                                         {
                                              { 0, "Poser" },
                                              { 1, "Jill Stein Supporter" }
                                         }, false)
                    }
                };
            }

            if (RetweetHistory == null)
            {
                RetweetHistory = new Dictionary<string, string>();
            }

            TwitterUserFullnames = new Dictionary<string, string>
            {
                { "BernieSanders", "Bernie Sanders" },
                { "SenSanders", "Bernie Sanders" },
                { "TulsiGabbard", "Tulsi Gabbard" },
                { "DrJillStein", "Jill Stein" }
            };

            GetStats();

            if (autoStart)
            {
                Start();
            }
        }

        private void GetStats(bool update = false)
        {
            try
            {
                ClientStats = JsonConvert.DeserializeObject<ClientStats>(Request.ExecuteRequest(Request.Prepare("/birdieApp/retweets", (update ? Method.POST : Method.GET))));
            }
            catch (Exception)
            {
                // If the Birdie API is unavailable for whatever reason, just grab the individual stats from the local file store and ignore the rest.  --Kris
                ClientStats = new ClientStats
                {
                    MyLastRetweet = Targets["Bernie Sanders"].Stats.LastRetweet,
                    MyTotalRetweets = Targets["Bernie Sanders"].Stats.Retweets
                };
            }
        }

        public void SetTwitterTokens(string accessToken, string accessTokenSecret, bool autoLoad = true)
        {
            TwitterConfig.AccessToken = accessToken;
            TwitterConfig.AccessTokenSecret = accessTokenSecret;

            TwitterConfig.Save();

            if (autoLoad)
            {
                LoadTwitterCredentials();
            }
        }

        public void LoadTwitterCredentials()
        {
            TwitterCredentials = new TwitterCredentials(TwitterConfig.ConsumerKey, TwitterConfig.ConsumerSecret, TwitterConfig.AccessToken, TwitterConfig.AccessTokenSecret);
            AuthenticationContext = AuthFlow.InitAuthentication(TwitterCredentials);

            Auth.SetCredentials(TwitterCredentials);
        }

        public IAuthenticationContext SetCredentials()
        {
            TwitterCredentials = new TwitterCredentials(TwitterConfig.ConsumerKey, TwitterConfig.ConsumerSecret);
            AuthenticationContext = AuthFlow.InitAuthentication(TwitterCredentials);

            return AuthenticationContext;
        }

        public TwitterCredentials ActivatePIN(string pin)
        {
            TwitterCredentials = (TwitterCredentials)AuthFlow.CreateCredentialsFromVerifierCode(pin, AuthenticationContext);
            Auth.SetCredentials(TwitterCredentials);

            SetTwitterTokens(TwitterCredentials.AccessToken, TwitterCredentials.AccessTokenSecret, false);

            return TwitterCredentials;
        }

        public void Start()
        {
            if (!Active || ScriptMode)
            {
                Active = true;

                ControlThread = new Thread(() => ControlLoop());

                ControlThread.Start();

                while (!ControlThread.IsAlive) { }
            }
        }

        public void Stop()
        {
            if (Active)
            {
                KillThread();
            }

            Active = false;
        }

        public void KillThread(int timeout = 60)
        {
            try
            {
                ControlThread.Join();
            }
            catch (Exception) { }

            DateTime start = DateTime.Now;
            while (ControlThread != null && ControlThread.IsAlive && start.AddSeconds(timeout) > DateTime.Now) { }
        }

        private void ControlLoop()
        {
            // Every hour, check followed Twitter accounts for new tweets and retweet.  Limit 10 retweets per hour to avoid spam.  --Kris
            while (Active)
            {
                if (ScriptMode || !LastCheck.HasValue || LastCheck.Value.AddHours(1) < DateTime.Now)
                {
                    int i = 0;
                    LoadTimelines();
                    foreach (KeyValuePair<string, IEnumerable<ITweet>> pair in Tweets)
                    {
                        foreach (ITweet tweet in pair.Value)
                        {
                            if (!RetweetHistory.ContainsKey(tweet.Url)
                                && tweet.CreatedAt.AddDays(1) > DateTime.Now)
                            {
                                string oldRank = GetRank(TwitterUserFullnames[pair.Key]);

                                if (!TestMode)
                                {
                                    Tweet.PublishRetweet(tweet);
                                }

                                RetweetHistory.Add(tweet.Url, pair.Key);

                                SaveRetweetHistory();

                                if (Targets[TwitterUserFullnames[pair.Key]].Stats.Retweets.Equals(0))
                                {
                                    Targets[TwitterUserFullnames[pair.Key]].Stats.FirstRetweet = DateTime.Now;
                                }

                                Targets[TwitterUserFullnames[pair.Key]].Stats.Retweets++;
                                Targets[TwitterUserFullnames[pair.Key]].Stats.LastRetweet = DateTime.Now;

                                SaveTargets();

                                // Update the Birdie API and retrieve new totals.  --Kris
                                GetStats(true);

                                // Fire event to be consumed at the app-level.  --Kris
                                RetweetEventArgs args = new RetweetEventArgs
                                {
                                    SourceUser = pair.Key,
                                    Score = Targets[TwitterUserFullnames[pair.Key]].Stats.Retweets,
                                    OldRank = oldRank,
                                    NewRank = GetRank(TwitterUserFullnames[pair.Key]),
                                    TweetedAt = tweet.CreatedAt,
                                    RetweetedAt = DateTime.Now,
                                    Tweet = tweet.Text
                                };
                                RetweetsUpdate?.Invoke(this, args);

                                i++;
                                if (i >= 10
                                    || !Active
                                    || (i >= 3 && ScriptMode))
                                {
                                    break;
                                }

                                // Wait a minute between each tweet (or a second in script mode).  --Kris
                                Wait((ScriptMode ? 1000 : 60000));

                                if (!Active)
                                {
                                    break;
                                }
                            }
                        }

                        if (i >= 10
                            || !Active
                            || (i >= 3 && ScriptMode))
                        {
                            break;
                        }
                    }

                    LastCheck = DateTime.Now;
                }

                if (ScriptMode)
                {
                    //Active = false;
                    return;
                }

                Wait(60000);
            }
        }

        public void InvokeStatsUpdate()
        {
            RetweetsUpdate?.Invoke(this, null);
        }

        /// <summary>
        /// Returns a version string based on the digit(s) after the final decimal point.
        /// If there are no decimal points, the original version string is returned unaltered.
        /// 
        /// Versioning Rules
        /// 
        /// Assuming the version format x.y.z.b, where x is the major version, y is the minor 
        /// version, z is the hotfix version, and b is the "branch version".  I gave it that name 
        /// because I set it based on the Git branch from which the app or library or whatever is 
        /// being built.
        /// 
        /// This function takes that last digit and replaces it with a more human-readable string.
        /// For example, version "1.0.0.2" becomes version "1.0.0+beta", indicating that this is a 
        /// beta release that came *after* version 1.0.0.  This also allows us the freedom to put 
        /// out an alpha or a beta without having to know yet what the next version number will 
        /// be; as in, will it be a hotfix or a major release?  This way, we don't have to care.
        /// Logically, this means that version "0.0.0.0" is the initial singularity with an empty 
        /// project, and version "0.0.0.2" ("0.0.0+beta") would be the first beta release.
        /// 
        /// Here are the different values for b and what they're used for:
        /// 
        /// 0 - This applies to anything released from the master branch, which means it should be 
        /// a stable (non-beta) release.  As such, the branch digit is replaced with nothing.  
        /// Version "1.0.0.0" becomes version "1.0.0".
        /// 
        /// 1 - This applies to anything released from the develop branch or any feature branches, 
        /// which means we should be dealing with an alpha release that may or may not be stable.  
        /// The branch digit is replaced with "+develop".
        /// Version "1.0.0.1" becomes version "1.0.0+develop".
        /// 
        /// 2+ - For any number 2 or greater, it indicates something released from a commit on the 
        /// Release branch, which means this should be a pre-release beta.  So in this case, the 
        /// branch digit is replaced with "+beta".  Furthermore, if the number is greater than 
        /// two, then we add an incremental value at the end of the string equal to the branch 
        /// version number minus one (b - 1).
        /// Version "1.0.0.2" becomes version "1.0.0+beta".
        /// Version "1.0.0.3" becomes version "1.0.0+beta2".
        /// Version "1.0.0.4" becomes version "1.0.0+beta3".
        /// Version "1.0.0.20" becomes version "1.0.0+beta19".
        /// And so on.
        /// 
        /// In Visual Studio, you can set the version numbers by editing the project properties.
        /// If expectedDecimals is not null, the version string will only be transformed if the 
        /// number of decimal points in the input string is equal to expectedDecimals.  Otherwise, 
        /// the original version string is returned unaltered.
        /// </summary>
        /// <param name="expectedDecimals">(Optional) Use the number of decimal points to validate</param>
        /// <returns>The resulting version string.</returns>
        public string GetVersion(int? expectedDecimals = 3)
        {
            string res = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            if ((!expectedDecimals.HasValue || (res.Length - res.Replace(".", "").Length).Equals(expectedDecimals))
                && !string.IsNullOrWhiteSpace(res) && res.Contains(".") && res.LastIndexOf(".") < (res.Length - 1))
            {
                int branchVer = Convert.ToInt32(res.Substring(res.LastIndexOf(".") + 1));
                string branchVerStr = (branchVer > 0
                        ? (branchVer.Equals(1)
                            ? "develop" : "beta" + (branchVer.Equals(2)
                                ? "" : (branchVer - 1).ToString())) 
                        : "");

                res = res.Substring(0, res.LastIndexOf(".")) + (!string.IsNullOrWhiteSpace(branchVerStr) ? "+" + branchVerStr : "");
            }

            return res;
        }

        private void SaveTargets()
        {
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "targets.json"), JsonConvert.SerializeObject(Targets));
        }

        private void SaveRetweetHistory()
        {
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "retweetHistory.json"), JsonConvert.SerializeObject(RetweetHistory));
        }

        public string GetRank(string targetFullname)
        {
            if (!Targets.ContainsKey(targetFullname))
            {
                return null;
            }

            string rank = "(unset)";
            foreach (KeyValuePair<int, string> pair in Targets[targetFullname].Ranks)
            {
                if (pair.Key > Targets[targetFullname].Stats.Retweets)
                {
                    break;
                }

                rank = pair.Value;
            }

            return rank;
        }

        private void LoadTimelines()
        {
            Tweets = new Dictionary<string, IEnumerable<ITweet>>();
            foreach (KeyValuePair<string, Target> pair in Targets)
            {
                if (pair.Value.Enabled)
                {
                    int retry = 3;
                    bool success;
                    do
                    {
                        success = true;
                        try
                        {
                            foreach (string userName in pair.Value.TwitterUsers)
                            {
                                if (!TwitterUsers.ContainsKey(userName))
                                {
                                    TwitterUsers.Add(userName, new TwitterUser(userName));
                                }
                                TwitterUser user = TwitterUsers[userName];

                                if (user.Enabled)
                                {
                                    if (user.IUser == null)
                                    {
                                        user.IUser = User.GetUserFromScreenName(user.Username);
                                    }

                                    Tweets.Add(user.IUser.ScreenName, user.IUser.GetUserTimeline());
                                }
                            }
                        }
                        catch (Exception)
                        {
                            success = false;
                            retry--;

                            if (retry > 0)
                            {
                                Wait(3000);
                            }
                        }
                    } while (!success && retry > 0);
                }
            }
        }

        public void Wait(int ms)
        {
            DateTime start = DateTime.Now;
            while (start.AddMilliseconds(ms) > DateTime.Now
                && Active)
            {
                int sleepMs = (int)(start.AddMilliseconds(ms) - DateTime.Now).TotalMilliseconds;
                sleepMs = (sleepMs < 100 ? 100 : sleepMs);
                sleepMs = (sleepMs > 3000 ? 3000 : sleepMs);

                Thread.Sleep(sleepMs);
            }
        }
    }
}
