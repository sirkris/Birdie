using Android.App;
using Android.Content;
using Android.OS;
using System;

namespace Birdie.Droid
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { Intent.ActionBootCompleted, "android.intent.action.QUICKBOOT_POWERON" },
        Categories = new[] { "android.intent.category.DEFAULT" })]
    public class RepeatingAlarm : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            //Toast.MakeText(context, "Received intent!", ToastLength.Short).Show();

            AndroidRefreshes.Alarm = DateTime.Now;

            // Launch BirdieLib ControlLoop in script mode.  --Kris
            try
            {
                Shared.BirdieLib.Start();
            }
            catch (Exception) { }

            // Repeat in ~15 minutes.  --Kris
            PendingIntent pendingIntent = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.UpdateCurrent);

            AlarmManager alarmManager = (AlarmManager)Application.Context.GetSystemService(Context.AlarmService);
            alarmManager.SetExactAndAllowWhileIdle(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime() + (15 * 1000 * 60), pendingIntent);
        }
    }
}