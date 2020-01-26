using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Work;
using Birdie.EventArgs;
using System;

namespace Birdie.Droid
{
    [Activity(Label = "Birdie")]
    public class BaseActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public Intent AlarmIntent { get; set; }
        public MainPage MainPage { get; set; }

        private PeriodicWorkRequest PeriodicWorkRequest { get; set; }

        private const int JOB_ID = 237643;

        // When the button is clicked, check if we're scheduled and fire the event.  --Kris
        public void C_ButtonClicked(object sender, ButtonClickedEventArgs e)
        {
            AlarmActiveEventArgs args;
            if (!Shared.BirdieLib.Active)
            {
                args = StartBirdieLib();
            }
            else
            {
                args = StopBirdieLib();
            }

            // Fire the event to update the button.  --Kris
            MainPage.InvokeAlarmActive(args);
        }

        public AlarmActiveEventArgs StartBirdieLib()
        {
            AndroidRefreshes.Alarm = DateTime.Now;

            // Set the alarm.  --Kris
            PendingIntent pendingIntent = PendingIntent.GetBroadcast(Application.Context, 0, AlarmIntent, PendingIntentFlags.UpdateCurrent);
            AlarmManager alarmManager = (AlarmManager)Application.Context.GetSystemService(AlarmService);

            alarmManager.SetExactAndAllowWhileIdle(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime() + 5000, pendingIntent);

            // Start the WorkManager.  --Kris
            if (PeriodicWorkRequest == null)
            {
                PeriodicWorkRequest = PeriodicWorkRequest.Builder.From<BirdieWorker>(TimeSpan.FromMinutes(20)).Build();
            }
            PeriodicWorkRequest.Tags.Add("BirdieWorker");

            WorkManager.Instance.Enqueue(PeriodicWorkRequest);
            
            return new AlarmActiveEventArgs
            {
                IsScheduled = true
            };
        }

        public AlarmActiveEventArgs StopBirdieLib()
        {
            // Stop the WorkManager.  --Kris
            WorkManager.Instance.CancelAllWork();
            PeriodicWorkRequest = null;

            PendingIntent pendingIntent = PendingIntent.GetBroadcast(Application.Context, 0, AlarmIntent, PendingIntentFlags.UpdateCurrent);
            AlarmManager alarmManager = (AlarmManager)Application.Context.GetSystemService(AlarmService);

            try
            {
                alarmManager.Cancel(pendingIntent);
                pendingIntent.Cancel();
            }
            catch (Exception) { }

            Shared.BirdieLib.Stop();

            return new AlarmActiveEventArgs
            {
                IsScheduled = false
            };
        }

        public bool IsScheduled()
        {
            return Shared.BirdieLib.Active;
        }
    }
}