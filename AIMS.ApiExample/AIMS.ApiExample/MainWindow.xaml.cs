using System;
using System.ComponentModel;
using System.Configuration;

namespace AIMS.ApiExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly Parameters _parameters;

        public MainWindow()
        {
            _parameters = ReadConfig();

            if (String.IsNullOrEmpty(_parameters.AccessToken))
            {
                new LoginWindow(_parameters).ShowDialog();
            }

            Provider = new EventProvider(_parameters);
            InitializeComponent();
        }

        public EventProvider Provider { get; private set; }

        public static void AddOrUpdateAppSettings(string key, string value)
        {
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            if (settings[key] == null)
            {
                settings.Add(key, value);
            }
            else
            {
                settings[key].Value = value;
            }
            configFile.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            AddOrUpdateAppSettings("AccessToken", _parameters.AccessToken);
            AddOrUpdateAppSettings("RefreshToken", _parameters.RefreshToken);

            base.OnClosing(e);
        }

        private static Parameters ReadConfig()
        {
            var config = ConfigurationManager.AppSettings;

            return new Parameters
            {
                AccessToken = config["AccessToken"],
                RefreshToken = config["RefreshToken"],
                ClientId = config["ClientId"],
                ClientSecret = config["ClientSecret"],
                AuthorizeUrl = config["AuthorizeUrl"],
                TokenUrl = config["TokenUrl"],
                MonitorUrl = config["MonitorUrl"],
            };
        }
    }
}