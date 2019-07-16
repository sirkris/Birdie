using System;

namespace BirdieLib
{
    public class BirdieLib
    {
        public bool Active { get; private set; }

        public BirdieLib()
        {
            Active = false;
        }

        public void Start()
        {
            Active = true;

            // TODO - Launch ControlLoop thread.  --Kris
        }

        public void Stop()
        {
            Active = false;

            // TODO - Stop thread.  --Kris
        }

        private void ControlLoop()
        {
            // TODO - Every hour, check followed Twitter accounts for new tweets and retweet.  --Kris
        }
    }
}
