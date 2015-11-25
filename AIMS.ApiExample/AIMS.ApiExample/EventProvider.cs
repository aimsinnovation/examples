using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace AIMS.ApiExample
{
    public class EventProvider : IDisposable
    {
        private readonly Parameters _parameters;
        private readonly Timer _timer;
        private long _lastEventId;

        public EventProvider(Parameters parameters)
        {
            Events = new ObservableCollection<Event>();
            Nodes = new ObservableCollection<Node>();
            _parameters = parameters;
            _timer = new Timer(OnTimerTick, null, 0, 30 * 1000);
        }

        public ObservableCollection<Event> Events { get; private set; }

        public ObservableCollection<Node> Nodes { get; private set; }

        public void Dispose()
        {
            _timer.Dispose();
        }

        private static string ConvertStatus(int status)
        {
            switch (status)
            {
                case 0x1:
                    return "open";

                case 0x2:
                    return "active";

                case 0x4:
                    return "ignored";

                case 0x8:
                    return "resolved";

                default:
                    return "unknown";
            }
        }

        private void AddEvents(IEnumerable<Event> events)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                foreach (Event e in events)
                {
                    Events.Add(e);
                }
            }));
        }

        private void AddNodes(IEnumerable<Node> nodes)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Nodes.Clear();
                foreach (Node n in nodes)
                {
                    Nodes.Add(n);
                }
            }));
        }

        private string CallApi(string path)
        {
            using (var client = new WebClient())
            {
                client.Headers["Authorization"] = "Bearer " + _parameters.AccessToken;

                try
                {
                    return client.DownloadString(new Uri(new Uri(_parameters.MonitorUrl), path));
                }
                catch (WebException ex)
                {
                    if (((HttpWebResponse)ex.Response).StatusCode != HttpStatusCode.Unauthorized) throw;

                    RefreshAccessToken();
                    client.Headers["Authorization"] = "Bearer " + _parameters.AccessToken;

                    return client.DownloadString(new Uri(new Uri(_parameters.MonitorUrl), path));
                }
            }
        }

        private void OnTimerTick(object state)
        {
            string eventsJson = _lastEventId == 0
                ? CallApi("events?take=5")
                : CallApi(String.Format("events?fromId={0}&take=1000", _lastEventId));

            Event[] events = JArray.Parse(eventsJson)
                .Cast<dynamic>()
                .Select(x => new Event
                {
                    Id = x.id,
                    Time = x.time,
                    Text = x.message,
                    Status = ConvertStatus((int)x.status),
                    Type = x.type == 2 ? "Warning" : "Error",
                })
                .ToArray();

            if (events.Any())
            {
                _lastEventId = events.Max(x => x.Id);
            }

            string nodesJson = CallApi("nodes");
            IEnumerable<Node> nodes = JArray.Parse(nodesJson)
                .Cast<dynamic>()
                .Select(x => new Node
                {
                    Status = x.status,
                    Name = x.name,
                })
                .Where(x => x.Status != 0);

            AddEvents(events);
            AddNodes(nodes);
        }

        private void RefreshAccessToken()
        {
            using (var client = new WebClient())
            {
                var requestQueryParams = new Dictionary<string, string>();
                requestQueryParams["grant_type"] = "refresh_token";
                requestQueryParams["refresh_token"] = _parameters.RefreshToken;
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
                    MessageBox.Show("Error during refreshing access token. Reason: " + error);
                    Application.Current.Shutdown();
                }

                _parameters.AccessToken = (string)response["access_token"];
            }
        }
    }
}