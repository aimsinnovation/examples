# API example

This sample project is provided here to demonstrate the basic use scenario of the AIMS API. For the full API documentation refer to [docs.aimsinnovation.com](https://docs.aimsinnovation.com/monitor).

### Running

To run the example application you need to build it (e.g. with Visual Studio 2010 or later) and edit the `App.config`. Specifically, there are three values you want to change:

```xml
  <appSettings>
    <add key="ClientId" value="{client_id}" />
    <add key="ClientSecret" value="{client_secret}" />
    ...
    <add key="MonitorUrl" value="{api_address}" />
  </appSettings>
```

 - `client_id` and `client_secret` – values you get when you register your application (refer to [OAuth2 specs](https://tools.ietf.org/html/rfc6749) for details, and write to support@aimsinnovation.com to register);
 - `api_address` – can be found on the Configuration page of the environment you want to connect to; looks like this: `https://{monitor_server}.aimsinnovation.com/monitors/{monitor_index}/api/`

### Authentication

The example application is using [OAuth2 authorization code grant flow](https://tools.ietf.org/html/rfc6749#section-4.1), but in a somewhat hacky way to work around the necessity of having a local web server running. Specifically, it asks for a redirect to www.example.com in the WPF built-in browser, and intercepts the redirection to grab the auth code. This is **not** a recommended way of using OAuth2 and is used here for the sake of simplicity.

Besides [OAuth2 authorization code grant flow](https://tools.ietf.org/html/rfc6749#section-4.1), AIMS API also supports [OAuth2 implicit flow](https://tools.ietf.org/html/rfc6749#section-4.2) as well as [basic authentication](https://tools.ietf.org/html/rfc2617#section-2).