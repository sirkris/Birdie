using System;
using System.Threading;

namespace BirdieLib
{
    public class BirdieLib
    {
        private DateTime? LastCheck;
        private Thread ControlThread;

        public bool Active { get; private set; }

        public BirdieLib()
        {
            Active = false;
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
                    // TODO

                    LastCheck = DateTime.Now;
                }

                Thread.Sleep(3000);
            }
        }
    }
}
