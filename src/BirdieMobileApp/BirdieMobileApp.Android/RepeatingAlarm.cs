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

namespace BirdieMobileApp.Droid
{
    [BroadcastReceiver]
    public class RepeatingAlarm : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Toast.MakeText(context, "Received intent!", ToastLength.Short).Show();
            // TODO - Launch BirdieLib ControlLoop in script mode.  --Kris

            // Repeat in ~15 minutes.  --Kris
            Intent intentNew = new Intent(context, typeof(RepeatingAlarm));
            PendingIntent pendingIntent = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.UpdateCurrent);

            AlarmManager alarmManager = (AlarmManager)Application.Context.GetSystemService(Context.AlarmService);
            alarmManager.SetExactAndAllowWhileIdle(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime() + (15 * 1000 * 60), pendingIntent);
        }
    }
}