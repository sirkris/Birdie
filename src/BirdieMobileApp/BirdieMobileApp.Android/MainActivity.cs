using BirdieMobileApp.EventArgs;
using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.LocalNotifications;
using Xamarin.Forms;

namespace BirdieMobileApp.Droid
{
    [Activity(Label = "BirdieMobileApp", Icon = "@mipmap/icon", Theme = "@style/MainTheme", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public Intent AlarmIntent { get; private set; }
        public MainPage MainPage { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Forms.Init(this, savedInstanceState);

            LocalNotificationsImplementation.NotificationIconId = Resource.Drawable.birdie;

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            AlarmIntent = new Intent(this, typeof(RepeatingAlarm));

            MainPage = new MainPage(Shared.BirdieLib);
            MainPage.ButtonClicked += C_ButtonClicked;
            
            LoadApplication(new App(MainPage));

            MainPage.InvokeAlarmActive(new AlarmActiveEventArgs { IsScheduled = !IsScheduled() });
        }

        // When the button is clicked, check if we're scheduled and fire the event.  --Kris
        public void C_ButtonClicked(object sender, ButtonClickedEventArgs e)
        {
            AlarmActiveEventArgs args = new AlarmActiveEventArgs
            {
                IsScheduled = IsScheduled()
            };
            
            PendingIntent pendingIntent = PendingIntent.GetBroadcast(this, 0, AlarmIntent, PendingIntentFlags.UpdateCurrent);
            AlarmManager alarmManager = (AlarmManager)Android.App.Application.Context.GetSystemService(AlarmService);

            if (args.IsScheduled)
            {
                alarmManager.Cancel(pendingIntent);
                pendingIntent.Cancel();
            }
            else
            {
                alarmManager.SetExactAndAllowWhileIdle(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime() + 5000, pendingIntent);
            }

            // Fire the event to update the button.  --Kris
            MainPage.InvokeAlarmActive(args);
        }

        public bool IsScheduled()
        {
            return (PendingIntent.GetBroadcast(this, 0, AlarmIntent, PendingIntentFlags.NoCreate) != null);
        }
    }
}
