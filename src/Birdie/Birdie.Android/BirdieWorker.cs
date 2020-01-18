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

using AndroidX.Work;

namespace Birdie.Droid
{
    public class BirdieWorker : Worker
    {
        public BirdieWorker(Context context, WorkerParameters workerParameters) : base(context, workerParameters) { }

        public override Result DoWork()
        {
            Shared.BirdieLib.Start();

            return Result.InvokeSuccess();
        }
    }
}
