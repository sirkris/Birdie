using BirdieLib.EventArgs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Tweetinvi;
using Tweetinvi.Models;

namespace BirdieLib
{
    public class BirdieLib
    {
        private DateTime? LastCheck;
        private Thread ControlThread;
        private Dictionary<string, IEnumerable<ITweet>> Tweets;

        private TwitterCredentials TwitterCredentials;
        private IAuthenticationContext AuthenticationContext;

        public event EventHandler<RetweetEventArgs> RetweetsUpdate;

        public Dictionary<string, Target> Targets;

        // Structure:  Original Tweet URL => Username
        public Dictionary<string, string> RetweetHistory;

        public TwitterConfig TwitterConfig;

        private readonly Dictionary<string, string> TwitterUserFullnames;
        private Dictionary<string, TwitterUser> TwitterUsers;

        // In test mode, everything functions normally except no retweets are actually sent out.  --Kris
        private readonly bool TestMode;

        public bool Active { get; private set; }

        public BirdieLib(bool testMode = false)
        {
            Active = false;

            TwitterUsers = new Dictionary<string, TwitterUser>();
            TestMode = testMode;

            // Load config, stats, and retweet history.  --Kris
            string targetsPath = Path.Combine(Environment.CurrentDirectory, "targets.json");
            if (File.Exists(targetsPath))
            {
                try
                {
                    Targets = JsonConvert.DeserializeObject<Dictionary<string, Target>>(File.ReadAllText(targetsPath));
                }
                catch (Exception) { }
            }

            string retweetHistoryPath = Path.Combine(Environment.CurrentDirectory, "retweetHistory.json");
            if (File.Exists(retweetHistoryPath))
            {
                try
                {
                    RetweetHistory = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(retweetHistoryPath));
                }
                catch (Exception) { }
            }

            TwitterConfig = new TwitterConfig(null);
            LoadTwitterCredentials();

            TweetinviConfig.CurrentThreadSettings.TweetMode = TweetMode.Extended;
            TweetinviConfig.CurrentThreadSettings.InitialiseFrom(TweetinviConfig.ApplicationSettings);

            // Only Bernie's tweets are monitored by default.  --Kris
            if (Targets == null)
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
        }

        private void LoadTwitterCredentials()
        {
            TwitterCredentials = new TwitterCredentials(TwitterConfig.ConsumerKey, TwitterConfig.ConsumerSecret, TwitterConfig.AccessToken, TwitterConfig.AccessTokenSecret);
            AuthenticationContext = AuthFlow.InitAuthentication(TwitterCredentials);

            Auth.SetCredentials(TwitterCredentials);
        }

        public void Start()
        {
            if (!Active)
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
                Active = false;

                KillThread();
            }
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
                if (!LastCheck.HasValue || LastCheck.Value.AddHours(1) < DateTime.Now)
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
                                if (i >= 10)
                                {
                                    break;
                                }

                                // Wait a minute between each tweet.  --Kris
                                Wait(60000);
                            }
                        }

                        if (i >= 10)
                        {
                            break;
                        }
                    }

                    LastCheck = DateTime.Now;
                }

                Wait(60000);
            }
        }

        private void SaveTargets()
        {
            File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "targets.json"), JsonConvert.SerializeObject(Targets));
        }

        private void SaveRetweetHistory()
        {
            File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "retweetHistory.json"), JsonConvert.SerializeObject(RetweetHistory));
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
