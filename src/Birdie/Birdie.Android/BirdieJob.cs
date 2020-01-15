using System.Threading.Tasks;
using Android.App;
using Android.App.Job;

namespace Birdie.Droid
{
    [Service(Name = "com.companyname.Birdie.Android.BirdieJob",
             Permission = "android.permission.BIND_JOB_SERVICE")]
    public class BirdieJob : JobService
    {
        #region Base Overrides
        public override void OnCreate()
        {
            base.OnCreate();
        }

        public override bool OnStartJob(JobParameters jobParams)
        {
            Task.Run(() =>
            {
                // Launch BirdieLib ControlLoop in script mode.  Run time is typically less than 10 seconds.  --Kris
                Shared.BirdieLib.Start();

                System.Threading.Thread.Sleep(10000);

                // Have to tell the JobScheduler the work is done. 
                JobFinished(jobParams, true);
            });

            // Return true because of the asynchronous work
            return true;
        }

        public override bool OnStopJob(JobParameters jobParams)
        {
            // we don't want to reschedule the job if it is stopped or cancelled.
            return false;
        }
        #endregion
    }
}
