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

namespace Birdie.AndroidMonitor
{
    [BroadcastReceiver]
    [IntentFilter(new[] { Intent.ActionBootCompleted })]
    public class BootCompleteReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action.Equals(Intent.ActionBootCompleted))
            {
                Toast.MakeText(context, "Received intent!", ToastLength.Short).Show();

            }
            else
            {
                Toast.MakeText(context, "Received intent (FAIL)!", ToastLength.Short).Show();
            }
        }
    }
}