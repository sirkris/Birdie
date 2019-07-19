using Tweetinvi.Models;

namespace BirdieLib
{
    public class TwitterUser
    {
        public string Username;
        public IUser IUser;

        public TwitterUser(string username, IUser iUser)
        {
            Username = username;
            IUser = iUser;
        }
    }
}
