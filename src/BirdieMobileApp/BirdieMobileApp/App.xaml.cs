using BirdieMobileApp.EventArgs;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace BirdieMobileApp
{
    public partial class App : Application
    {
        public NavigationPage NavPage;

        public App(MainPage mainPage)
        {
            InitializeComponent();

            MainPage = new NavigationPage(mainPage);
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
