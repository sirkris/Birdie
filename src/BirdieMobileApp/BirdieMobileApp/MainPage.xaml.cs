using BirdieLib.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BirdieMobileApp
{
    public partial class MainPage : ContentPage
    {
        private BirdieLib.BirdieLib BirdieLib;

        public MainPage()
        {
            InitializeComponent();
            BirdieLib = new BirdieLib.BirdieLib();
            labelVersion.Text = "Version " + BirdieLib.GetVersion();
            myRetweets.Text = "My Retweets: " + BirdieLib.Targets["Bernie Sanders"].Stats.Retweets.ToString();
            lastRetweet.Text = "Last Retweet: " + (BirdieLib.Targets["Bernie Sanders"].Stats.LastRetweet.HasValue ? BirdieLib.Targets["Bernie Sanders"].Stats.LastRetweet.Value.ToString("G") : "N/A");
            myRank.Text = "Current Rank: " + BirdieLib.GetRank("Bernie Sanders");
        }

        void Button_Clicked(object sender, EventArgs e)
        {
            if (BirdieLib.Active)
            {
                BirdieLib.Stop();
                BirdieLib.RetweetsUpdate -= C_StatsUpdated;

                ((Button)sender).Text = "Start";
                ((Button)sender).TextColor = Color.LawnGreen;
                ((Button)sender).BackgroundColor = Color.DarkGreen;
            }
            else
            {
                BirdieLib.Start();
                BirdieLib.RetweetsUpdate += C_StatsUpdated;

                ((Button)sender).Text = "Stop";
                ((Button)sender).TextColor = Color.Yellow;
                ((Button)sender).BackgroundColor = Color.Red;
            }
        }

        public void C_StatsUpdated(object sender, RetweetEventArgs e)
        {
            // TODO - Update stats displayed and activate notification if new rank is achieved.  --Kris
        }
    }
}
