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
        public ClientStats ClientStats { get; private set; }

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

        public string GetVersion()
        {
            string res = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return (string.IsNullOrWhiteSpace(res) || !res.Contains(".") ? res : res.Substring(0, res.LastIndexOf(".")) +
                (res.EndsWith(".1") ? "+develop" : res.EndsWith(".2") ? "+beta" : ""));
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
