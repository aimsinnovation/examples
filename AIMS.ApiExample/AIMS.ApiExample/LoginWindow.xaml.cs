using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Navigation;
using Newtonsoft.Json.Linq;

namespace AIMS.ApiExample
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow
    {
        private readonly Parameters _parameters;

        public LoginWindow(Parameters parameters)
        {
            _parameters = parameters;
            InitializeComponent();

            Browser.Source = new Uri(String.Format("{0}?client_id={1}&response_type=code&redirect_uri=www.example.com",
                parameters.AuthorizeUrl, parameters.ClientId));
        }

        private void OnClosed(object sender, EventArgs e)
        {
            if (DialogResult == true) return;

            Application.Current.Shutdown();
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            Browser.Visibility = Visibility.Visible;
        }

        private void OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            Browser.Visibility = Visibility.Collapsed;

            if (e.Uri.Host != "www.example.com") return;

            var queryParams = e.Uri.Query.TrimStart('?').Split('&')
                .Select(x => x.Split('='))
                .ToDictionary(x => x[0], x => x[1]);

            if (!queryParams.ContainsKey("code"))
            {
                Application.Current.Shutdown();
            }

            using (var client = new WebClient())
            {
                var requestQueryParams = new Dictionary<string, string>();
                requestQueryParams["grant_type"] = "authorization_code";
                requestQueryParams["code"] = queryParams["code"];
                requestQueryParams["client_id"] = _parameters.ClientId;
                requestQueryParams["client_secret"] = _parameters.ClientSecret;
                requestQueryParams["redirect_uri"] = "www.example.com";

                string uriString = String.Format("{0}?{1}", _parameters.TokenUrl,
                    String.Join("&", requestQueryParams.Select(x => x.Key + "=" + x.Value)));

                string s = client.UploadString(uriString, "POST", "");
                var response = JObject.Parse(s);
                var error = (string)response["error"];
                if (error != null)
                {
                    MessageBox.Show("Error during getting access token. Reason: " + error);
                    Application.Current.Shutdown();
                }

                _parameters.AccessToken = (string)response["access_token"];
                _parameters.RefreshToken = (string)response["refresh_token"];
                DialogResult = true;
            }
        }
    }
}