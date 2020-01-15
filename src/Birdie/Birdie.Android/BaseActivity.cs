using Birdie.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.App.Job;

namespace Birdie.Droid
{
    [Activity(Label = "Birdie")]
    public class BaseActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public Intent AlarmIntent { get; set; }
        public MainPage MainPage { get; set; }

        private const int JOB_ID = 237643;

        // When the button is clicked, check if we're scheduled and fire the event.  --Kris
        public void C_ButtonClicked(object sender, ButtonClickedEventArgs e)
        {
            JobScheduler jobScheduler = (JobScheduler)GetSystemService(JobSchedulerService);
            AlarmActiveEventArgs args;
            if (!Shared.BirdieLib.Active)
            {
                // Run the BirdieLib (in script mode) once now since the scheduler won't fire for 15 minutes.  --Kris
                //Shared.BirdieLib.Start();

                // Set a recurring JobScheduler to take it from here.  --Kris
                JobInfo.Builder builder = new JobInfo.Builder(JOB_ID, new ComponentName(this, Java.Lang.Class.FromType(typeof(BirdieJob))));

                builder.SetPeriodic(900000);  // Fire every ~15 minutes.
                builder.SetPersisted(true);  // Persist across reboots.

                builder.SetMinimumLatency(0);
                //builder.SetOverrideDeadline(15 * 1000);
                builder.SetBackoffCriteria(60 * 1000, BackoffPolicy.Linear);
                builder.SetRequiredNetworkType(NetworkType.Any);

                JobInfo jobInfo = builder.Build();

                if (!jobScheduler.Schedule(jobInfo).Equals(JobScheduler.ResultSuccess))
                {
                    throw new Exception("Failed to start JobScheduler!");
                }

                args = new AlarmActiveEventArgs
                {
                    IsScheduled = true
                };
            }
            else
            {
                // Cancel the job.  --Kris
                if (jobScheduler != null)
                {
                    jobScheduler.Cancel(JOB_ID);
                }

                Shared.BirdieLib.Stop();

                args = new AlarmActiveEventArgs
                {
                    IsScheduled = false
                };
            }

            /*
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
            */

            // Fire the event to update the button.  --Kris
            MainPage.InvokeAlarmActive(args);
        }

        public bool IsScheduled()
        {
            //return (PendingIntent.GetBroadcast(this, 0, AlarmIntent, PendingIntentFlags.NoCreate) != null);
            return Shared.BirdieLib.Active;
        }
    }
}