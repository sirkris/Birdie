using Birdie.EventArgs;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Birdie
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TwitterAuth : ContentPage
    {
        private string PageSrc;
        private MainPage MainPage;

        private readonly string AuthURL;

        public TwitterAuth(MainPage mainPage)
        {
            InitializeComponent();

            MainPage = mainPage;
            AuthURL = Shared.BirdieLib.SetCredentials().AuthorizationURL;

            BrowserWindow.Source = AuthURL;
            BrowserWindow.Navigated += C_NavigatedUpdated;
        }

        /*
         * TODO
         * 
         * This worked fine in VS 2017.  After I upgraded to 2019, however, it only retrieves the page source 
         * for the initial auth page, even after clicking the button.  Nothing I do seems to change this odd behavior 
         * and I can find no alternative approaches to extracting page source from a WebView.
         * 
         * Until a fix can be found, the user will have to manually enter the PIN.
         * 
         * --Kris
         */
        public async void C_NavigatedUpdated(object sender, WebNavigatedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Url) && !e.Url.Equals(AuthURL))
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
                else if (string.IsNullOrWhiteSpace(PageSrc))
                {
                    await DisplayAlert("Twitter Authentication Failed", "Unable to retrieve page source.  Please restart the app and try again.", "Damnit!");
                    Application.Current.Quit();
                }
                else
                {
                    // Since the Xamarin.Forms WebView will no longer cough up the correct page source, the user will have to enter the PIN manually.  --Kris

                    // Uncomment below when the WebView source retrieval bug has been fixed.  --Kris
                    
                    await DisplayAlert("Twitter Authentication Failed", "Unable to retrieve PIN from page source.  Please restart the app and try again.", "Damnit!");
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