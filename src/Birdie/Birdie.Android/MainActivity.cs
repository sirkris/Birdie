using Birdie.EventArgs;
using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.LocalNotifications;
using Xamarin.Forms;

namespace Birdie.Droid
{
    [Activity(Label = "Birdie", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : BaseActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Forms.Init(this, savedInstanceState);

            LocalNotificationsImplementation.NotificationIconId = Resource.Drawable.birdie;

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            //AlarmIntent = new Intent(this, typeof(RepeatingAlarm));

            MainPage = new MainPage(Shared.BirdieLib);
            MainPage.ButtonClicked += C_ButtonClicked;

            LoadApplication(new App(MainPage));

            MainPage.InvokeAlarmActive(new AlarmActiveEventArgs { IsScheduled = IsScheduled() });
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}