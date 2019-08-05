using BirdieLib.EventArgs;
using Plugin.LocalNotifications;
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

            RefreshRetweets();
            RefreshLastRetweet();
            RefreshRank();
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

        private void RefreshRetweets()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                otherStats.Text = "My Retweets: " + BirdieLib.Targets["Bernie Sanders"].Stats.Retweets.ToString()
                                 + Environment.NewLine
                                 + "Total Retweets: " + BirdieLib.ClientStats.TotalRetweets.ToString()
                                 + Environment.NewLine
                                 + "Active Users: " + BirdieLib.ClientStats.ActiveUsers.ToString() + " / " + BirdieLib.ClientStats.TotalUsers.ToString();
            });
        }

        private void RefreshLastRetweet()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                lastRetweet.Text = "Last Retweet: " + (BirdieLib.Targets["Bernie Sanders"].Stats.LastRetweet.HasValue ? BirdieLib.Targets["Bernie Sanders"].Stats.LastRetweet.Value.ToString("g") : "N/A");
            });
        }

        private void RefreshRank()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                myRank.Text = "Current Rank: " + BirdieLib.GetRank("Bernie Sanders");
            });
        }

        public void C_StatsUpdated(object sender, RetweetEventArgs e)
        {
            // Update stats displayed and activate notification if new rank is achieved.  --Kris
            RefreshRetweets();
            RefreshLastRetweet();
            RefreshRank();

            if (!e.NewRank.Equals(e.OldRank)
                && (e.SourceUser.Equals("BernieSanders") || e.SourceUser.Equals("SenSanders")))
            {
                // Fire notification of new rank.  --Kris
                CrossLocalNotifications.Current.Show("New Rank", "Congratulations! You've earned a new rank: " + e.NewRank);
            }
        }
    }
}
