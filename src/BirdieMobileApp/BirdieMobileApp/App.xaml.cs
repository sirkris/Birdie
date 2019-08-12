using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace BirdieMobileApp
{
    public partial class App : Application
    {
        public NavigationPage NavPage;

        public App()
        {
            InitializeComponent();

            BirdieLib.BirdieLib birdieLib = new BirdieLib.BirdieLib();

            MainPage = new NavigationPage(new MainPage(birdieLib));
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
