
using Android.App;
using Android.Content;

namespace Birdie.Droid
{
    [BroadcastReceiver]
    [IntentFilter(new[] { Intent.ActionBootCompleted })]
    public class BootReceiver : BroadcastReceiver
    {
        #region implemented abstract members of BroadcastReceiver
        public override void OnReceive(Context context, Intent intent)
        {
            /*if (intent.Action.Equals(Intent.ActionBootCompleted))
            {
                Toast.MakeText(context, "Received BOOT intent!", ToastLength.Short).Show();
                // TODO - Try replacing the service with a JobScheduler instead?  --Kris
                Intent serviceIntent = new Intent(context, typeof(BootService));
                serviceIntent.AddFlags(ActivityFlags.NewTask);

                context.StartService(serviceIntent);
            }
            else
            {
                Toast.MakeText(context, "ERROR: Received INCORRECT BOOT intent : " + intent.Action, ToastLength.Short).Show();
            }*/
        }
        #endregion
    }
}