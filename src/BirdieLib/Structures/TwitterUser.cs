using System;
using Tweetinvi;
using Tweetinvi.Models;

namespace BirdieLib
{
    [Serializable]
    public class TwitterUser
    {
        public string Username { get; set; }
        public IUser IUser { get; set; }
        public bool Enabled { get; set; }

        public TwitterUser(string username, bool enabled = true)
        {
            Username = username;
            if (enabled)
            {
                IUser = User.GetUserFromScreenName(username);
            }
            Enabled = enabled;
        }
    }
}
