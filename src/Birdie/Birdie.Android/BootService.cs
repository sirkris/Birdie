
using Android.App;
using Android.Content;
using Android.OS;

namespace Birdie.Droid
{
    [Service]
    public class BootService : Service
    {
        public Intent AlarmIntent { get; set; }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            /*AlarmIntent = new Intent(this, typeof(RepeatingAlarm));

            PendingIntent pendingIntent = PendingIntent.GetBroadcast(this, 0, AlarmIntent, PendingIntentFlags.UpdateCurrent);
            AlarmManager alarmManager = (AlarmManager)Android.App.Application.Context.GetSystemService(AlarmService);

            if (IsScheduled())
            {
                alarmManager.Cancel(pendingIntent);
                pendingIntent.Cancel();
            }
            else
            {
                alarmManager.SetExactAndAllowWhileIdle(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime() + 5000, pendingIntent);
            }*/

            return StartCommandResult.Sticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public bool IsScheduled()
        {
            return (PendingIntent.GetBroadcast(this, 0, AlarmIntent, PendingIntentFlags.NoCreate) != null);
        }
    }
}
