using BirdieLib.EventArgs;
using BirdieMobileApp.EventArgs;
using Plugin.LocalNotifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BirdieMobileApp
{
    public partial class MainPage : ContentPage
    {
        private BirdieLib.BirdieLib BirdieLib;
        private DateTime? PressedStart;

        public event EventHandler<ButtonClickedEventArgs> ButtonClicked;
        public event EventHandler<AlarmActiveEventArgs> AlarmActive;

        public MainPage(BirdieLib.BirdieLib birdieLib)
        {
            InitializeComponent();

            BirdieLib = birdieLib;

            labelVersion.Text = "Version " + BirdieLib.GetVersion();

            RefreshRetweets();
            RefreshLastRetweet();
            RefreshRank();

            if (string.IsNullOrWhiteSpace(BirdieLib.TwitterConfig.AccessToken) || string.IsNullOrWhiteSpace(BirdieLib.TwitterConfig.AccessTokenSecret))
            {
                Navigation.PushAsync(new TwitterAuth(BirdieLib));
            }

            //BirdieLib.StatusUpdate += C_ActiveUpdated;  // Deprecated; fires when BirdieLib.Active changes and we're no longer tracking to that on mobile because of scheduler.  --Kris
            AlarmActive += C_AlarmActive;
            BirdieLib.RetweetsUpdate += C_StatsUpdated;
        }

        public void InvokeAlarmActive(AlarmActiveEventArgs args)
        {
            AlarmActive?.Invoke(this, args);
        }

        private void UpdateButton(bool active)
        {
            if (active)
            {
                StartButton.Text = "Stop";
                StartButton.TextColor = Color.Yellow;
                StartButton.BackgroundColor = Color.Red;
            }
            else
            {
                StartButton.Text = "Start";
                StartButton.TextColor = Color.LawnGreen;
                StartButton.BackgroundColor = Color.DarkGreen;
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

        public void C_ActiveUpdated(object sender, StatusUpdateEventArgs e)
        {
            UpdateButton(e.Active);
        }

        public void C_AlarmActive(object sender, AlarmActiveEventArgs e)
        {
            UpdateButton(!e.IsScheduled);
        }

        private void StartButton_Pressed(object sender, System.EventArgs e)
        {
            PressedStart = DateTime.Now;
        }

        private void StartButton_Released(object sender, System.EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BirdieLib.TwitterConfig.AccessToken)
                || string.IsNullOrWhiteSpace(BirdieLib.TwitterConfig.AccessTokenSecret)
                || (!BirdieLib.Active && PressedStart.HasValue && PressedStart.Value.AddSeconds(10) <= DateTime.Now))
            {
                // Hold the Start button for 10 seconds to clear Twitter credentials and return to auth screen.  --Kris
                BirdieLib.TwitterConfig.Clear();
                BirdieLib.TwitterConfig.Save();

                Navigation.PushAsync(new TwitterAuth(BirdieLib));
            }
            else
            {
                // Fire event that tells Android/iOS apps that the start/stop button was pressed.  --Kris
                ButtonClickedEventArgs args = new ButtonClickedEventArgs
                {
                    ClickedAt = DateTime.Now
                };
                ButtonClicked?.Invoke(this, args);
                /*
                if (BirdieLib.Active)
                {
                    BirdieLib.Stop();
                }
                else
                {
                    BirdieLib.Start();
                }
                */
            }
        }
    }
}
