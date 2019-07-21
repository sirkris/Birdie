using BirdieLib.EventArgs;
using System;
using System.Collections.Generic;
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

        public event EventHandler<RetweetEventArgs> RetweetsUpdate;

        public Dictionary<string, Target> Targets;

        // Structure:  Original Tweet URL => Username
        public Dictionary<string, string> RetweetHistory;

        private readonly Dictionary<string, string> TwitterUserFullnames;

        public bool Active { get; private set; }

        public BirdieLib()
        {
            Active = false;

            // Only Bernie's tweets are monitored by default.  --Kris
            Targets = new Dictionary<string, Target>
            {
                {
                    "Bernie Sanders", new Target("Bernie Sanders", new List<TwitterUser>
                                      {
                                          new TwitterUser("BernieSanders"),
                                          new TwitterUser("SenSanders")
                                      },
                                      new Dictionary<int, string>  // The score value is the number of retweets.  --Kris
                                      {
                                          { 0, "Poser" },
                                          { 1, "Rookie Berner" },
                                          { 10, "Volunteer" },
                                          { 25, "Aspiring Revolutionary" },
                                          { 50, "Birdie Bro" },
                                          { 75, "Fictional Chair-Thrower" },
                                          { 100, "Berner" },
                                          { 250, "Social Media Soldier" },
                                          { 500, "Berner-Elite" },
                                          { 1000, "Revolutionary Legend" }
                                      })
                },
                {
                    "Tulsi Gabbard", new Target("Tulsi Gabbard", new List<TwitterUser> { new TwitterUser("TulsiGabbard", false) },
                                     new Dictionary<int, string>
                                     {
                                          { 0, "Poser" },
                                          { 1, "Tulsi Gabbard Supporter" }
                                     })
                },
                {
                    "Jill Stein", new Target("Jill Stein", new List<TwitterUser> { new TwitterUser("DrJillStein", false) },
                                     new Dictionary<int, string>
                                     {
                                          { 0, "Poser" },
                                          { 1, "Jill Stein Supporter" }
                                     })
                }
            };

            // TODO - Load stats and retweet history.  --Kris

            TwitterUserFullnames = new Dictionary<string, string>
            {
                { "BernieSanders", "Bernie Sanders" },
                { "SenSanders", "Bernie Sanders" },
                { "TulsiGabbard", "Tulsi Gabbard" },
                { "DrJillStein", "Jill Stein" }
            };
        }

        public void Start()
        {
            if (!Active)
            {
                Active = true;

                ControlThread = new Thread(() => ControlLoop());
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
            // Every hour, check followed Twitter accounts for new tweets and retweet.  --Kris
            while (Active)
            {
                if (!LastCheck.HasValue || LastCheck.Value.AddHours(1) < DateTime.Now)
                {
                    LoadTimelines();
                    foreach (KeyValuePair<string, IEnumerable<ITweet>> pair in Tweets)
                    {
                        foreach (ITweet tweet in pair.Value)
                        {
                            if (!RetweetHistory.ContainsKey(tweet.Url))
                            {
                                Tweet.PublishRetweet(tweet);
                                RetweetHistory.Add(tweet.Url, pair.Key);

                                if (Targets[TwitterUserFullnames[pair.Key]].Stats.Retweets.Equals(0))
                                {
                                    Targets[TwitterUserFullnames[pair.Key]].Stats.FirstRetweet = DateTime.Now;
                                }

                                string oldRank = GetRank(pair.Key);

                                Targets[TwitterUserFullnames[pair.Key]].Stats.Retweets++;
                                Targets[TwitterUserFullnames[pair.Key]].Stats.LastRetweet = DateTime.Now;

                                // Fire event to be consumed at the app-level.  --Kris
                                RetweetEventArgs args = new RetweetEventArgs
                                {
                                    SourceUser = pair.Key,
                                    Score = Targets[TwitterUserFullnames[pair.Key]].Stats.Retweets,
                                    OldRank = oldRank,
                                    NewRank = GetRank(pair.Key),
                                    TweetedAt = tweet.CreatedAt,
                                    RetweetedAt = DateTime.Now,
                                    Tweet = tweet.Text
                                };
                                RetweetsUpdate?.Invoke(this, args);
                            }
                        }
                    }

                    LastCheck = DateTime.Now;
                }

                Wait(60000);
            }
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
                foreach (TwitterUser user in pair.Value.TwitterUsers)
                {
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
