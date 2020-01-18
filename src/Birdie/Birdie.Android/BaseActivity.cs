using Birdie.EventArgs;
using System;

using Android.App;
using Android.Content;
using Android.App.Job;

using AndroidX.Work;

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

                /*
                 * TODO
                 * 
                 * This stops working after a set amount of time.  Passing true to JobFinished didn't solve the issue (should be false anyway).
                 * May be caused by additional OS restrictions relating to SetPeriodic.  Will try replacing the JobScheduler with a 
                 * WorkManager and see if those internal optimizations do the trick.  Otherwise, I'll have to implement some sort of hybrid 
                 * approach with jobs and alarms spawing each other, assuming I can even do that.  Fucking Android.
                 * 
                 * --Kris
                 */
                // Set a recurring JobScheduler to take it from here.  --Kris
                /*JobInfo.Builder builder = new JobInfo.Builder(JOB_ID, new ComponentName(this, Java.Lang.Class.FromType(typeof(BirdieJob))));

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
                }*/

                // Start the WorkManager.  --Kris
                PeriodicWorkRequest periodicWorkRequest = PeriodicWorkRequest.Builder.From<BirdieWorker>(TimeSpan.FromMinutes(20)).Build();
                WorkManager.Instance.Enqueue(periodicWorkRequest);

                args = new AlarmActiveEventArgs
                {
                    IsScheduled = true
                };
            }
            else
            {
                // Cancel the job.  --Kris
                /*if (jobScheduler != null)
                {
                    jobScheduler.Cancel(JOB_ID);
                }

                Shared.BirdieLib.Stop();*/

                // Stop the WorkManager.  --Kris
                WorkManager.Instance.CancelAllWork();

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