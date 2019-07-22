using System;

namespace BirdieLib.EventArgs
{
    public class RetweetEventArgs
    {
        /// <summary>
        /// The username of the Twitter account we got the tweet from.
        /// </summary>
        public string SourceUser { get; set; }

        /// <summary>
        /// The number of SourceUser's tweets you've retweeted.
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Your rank as of the previous update.
        /// </summary>
        public string OldRank { get; set; }

        /// <summary>
        /// Your current rank.
        /// </summary>
        public string NewRank { get; set; }

        /// <summary>
        /// When the tweet was first posted.
        /// </summary>
        public DateTime TweetedAt { get; set; }

        /// <summary>
        /// When the tweet was retweeted.
        /// </summary>
        public DateTime RetweetedAt { get; set; }

        /// <summary>
        /// The text of the tweet.
        /// </summary>
        public string Tweet { get; set; }
    }
}
