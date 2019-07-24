using BirdieLib.EventArgs;
using System;

namespace BirdieConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // A very basic console app running BirdieLib.  --Kris
            BirdieLib.BirdieLib birdieLib = new BirdieLib.BirdieLib(args[0].Equals("testmode"));

            birdieLib.Start();
            birdieLib.RetweetsUpdate += C_StatsUpdated;

            Console.WriteLine("Birdie is now running" + (args[0].Equals("testmode") ? " in test mode" : "") + ".  Press any key to exit....");

            Console.ReadKey();

            birdieLib.RetweetsUpdate -= C_StatsUpdated;
            birdieLib.Stop();

            Console.WriteLine("Birdie has stopped.");
        }

        public static void C_StatsUpdated(object sender, RetweetEventArgs e)
        {
            Console.WriteLine("[" + DateTime.Now.ToString("F") + "]: Retweet #" + e.Score.ToString() + " from " + e.SourceUser + ": " + e.Tweet);

            if (!e.NewRank.Equals(e.OldRank)
                && (e.SourceUser.Equals("BernieSanders") || e.SourceUser.Equals("SenSanders")))
            {
                Console.WriteLine("************************************************************");
                Console.WriteLine("Congratulations!  You have earned a new rank:  " + e.NewRank);
                Console.WriteLine("************************************************************");
            }
            else
            {
                Console.WriteLine("[Current Rank: " + e.NewRank + "]");
            }
        }
    }
}
