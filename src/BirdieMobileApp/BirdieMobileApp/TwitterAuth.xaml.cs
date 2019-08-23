using BirdieMobileApp.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BirdieMobileApp
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TwitterAuth : ContentPage
    {
        private string PageSrc;
        private MainPage MainPage;

        public TwitterAuth(MainPage mainPage)
        {
            InitializeComponent();

            MainPage = mainPage;

            BrowserWindow.Source = Shared.BirdieLib.SetCredentials().AuthorizationURL;
            BrowserWindow.Navigating += C_NavigatingUpdated;
        }

        public async void C_NavigatingUpdated(object sender, WebNavigatingEventArgs e)
        {
            await GetWebViewSrc();
            if (!string.IsNullOrWhiteSpace(PageSrc) && PageSrc.Contains("PIN"))
            {
                // Grab PIN from PageSrc string then proceed with auth.  --Kris
                string pin = null;
                try
                {
                    pin = Regex.Match(PageSrc.Replace(@"\u003C", @"<"), @"<code>(.*)<\/code>").Groups[1].Value;
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Twitter Authentication Failed", "Exception thrown trying to obtain PIN: " + ex.Message, "OK");
                    Application.Current.Quit();
                }

                if (!string.IsNullOrWhiteSpace(Shared.BirdieLib.ActivatePIN(pin).AccessToken))
                {
                    await DisplayAlert("Twitter Authentication Complete",
                        "Setup complete!  You don't have to do anything else.  Birdie will now run automatically in the background.  Thank you for volunteering!",
                        "OK");

                    // Close this window, start Birdie, and return to main window.  --Kris
                    MainPage.InvokeButtonClicked(new ButtonClickedEventArgs { ClickedAt = DateTime.Now });
                    
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Twitter Authentication Failed", "An unknown error occured while trying to authenticate Twitter.", "Damnit!");
                    Application.Current.Quit();
                }
            }
        }

        private async Task GetWebViewSrc()
        {
            PageSrc = await BrowserWindow.EvaluateJavaScriptAsync("document.body.innerHTML");
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}
