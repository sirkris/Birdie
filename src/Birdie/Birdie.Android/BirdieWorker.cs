using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Work;
using System;
using System.Threading;

namespace Birdie.Droid
{
    [Service(Name = "com.companyname.Birdie.Android.BirdieWorker",
             Permission = "android.permission.BIND_JOB_SERVICE")]
    public class BirdieWorker : Worker
    {
        public Intent AlarmIntent { get; set; }

        private bool AlarmActive
        {
            get
            {
                return (AndroidRefreshes.Alarm.HasValue && AndroidRefreshes.Alarm.Value.AddHours(1) > DateTime.Now);
            }
            set { }
        }

        private bool AlarmSet
        {
            get
            {
                return (PendingIntent.GetBroadcast(Application.Context, 0, AlarmIntent, PendingIntentFlags.NoCreate) != null);
            }
            set { }
        }

        public BirdieWorker(Context context, WorkerParameters workerParameters) : base(context, workerParameters)
        {
            AlarmIntent = new Intent(Application.Context, typeof(RepeatingAlarm));
        }

        public override Result DoWork()
        {
            try
            {
                AndroidRefreshes.Worker = DateTime.Now;

                // If the alarm is not yet active or hasn't refreshed in awhile, attempt to recreate it, then wait for it to fire.  --Kris
                if (!AlarmActive || !AlarmSet)
                {
                    // Set the alarm.  --Kris
                    PendingIntent pendingIntent = PendingIntent.GetBroadcast(Application.Context, 0, AlarmIntent, PendingIntentFlags.UpdateCurrent);
                    AlarmManager alarmManager = (AlarmManager)Application.Context.GetSystemService(Context.AlarmService);

                    alarmManager.SetExactAndAllowWhileIdle(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime() + 5000, pendingIntent);

                    // Wait for the alarm intent to execute.  --Kris
                    DateTime start = DateTime.Now;
                    while (!AlarmActive
                        && start.AddSeconds(30) > DateTime.Now)
                    {
                        Thread.Sleep(5000);
                    }

                    // If the alarm still hasn't refreshed, fire the script from here, instead.  --Kris
                    if (!AlarmActive)
                    {
                        Shared.BirdieLib.Start();
                    }
                }
                // Otherwise, refresh data from the Birdie API for display purposes.  --Kris
                else
                {
                    Shared.BirdieLib.InvokeStatsUpdate();
                }
            }
            catch (Exception) { }

            return Result.InvokeSuccess();
        }
    }
}
