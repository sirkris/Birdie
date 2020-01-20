using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Android.App;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using AndroidX.Work;

namespace Birdie.Droid
{
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
            AndroidRefreshes.Worker = DateTime.Now;

            // If the alarm is not yet active or hasn't refreshed in awhile, attempt to recreate it, then wait for it to fire.  --Kris
            if (!AlarmActive || !AlarmSet)
            {
                // Set the alarm.  --Kris
                PendingIntent pendingIntent = PendingIntent.GetBroadcast(Application.Context, 0, AlarmIntent, PendingIntentFlags.UpdateCurrent);
                AlarmManager alarmManager = (AlarmManager)Android.App.Application.Context.GetSystemService(Context.AlarmService);

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

            return Result.InvokeSuccess();
        }
    }
}
