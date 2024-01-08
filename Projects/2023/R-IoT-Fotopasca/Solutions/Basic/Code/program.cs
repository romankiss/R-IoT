using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using nanoFramework.WebServer;
using System.Device.Wifi;
using nanoFramework.Networking;
using nanoFramework.AtomLite;

namespace websajt
{
    public class Program
    {
    public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            var neo = AtomLite.NeoPixel;
            neo.Image.SetPixel(0, 0, 0, 0, 15);
            neo.Update();

            const string Ssid = "Meno_Wifi-ny";
            const string Password = "heslo";
            // Give 60 seconds to the wifi join to happen
            CancellationTokenSource cs = new(60000);
            var success = WifiNetworkHelper.ConnectFixAddress(Ssid, Password, new IPConfiguration("192.168.1.7", "255.255.255.0", "192.168.1.1"), requiresDateTime: true, token: cs.Token);


            if (!success)
            {
                neo.Image.SetPixel(0, 0, 15, 0, 0);
                neo.Update();
                // Something went wrong, you can get details with the ConnectionError property:
                Debug.WriteLine($"Can't connect to the network, error: {WifiNetworkHelper.Status}");
                if (WifiNetworkHelper.HelperException != null)
                {
                    Debug.WriteLine($"ex: {WifiNetworkHelper.HelperException}");
                }
            }
            else
            {
                // Otherwise, you are connected and have a valid IP and date
                Debug.WriteLine($" {WifiNetworkHelper.Status}");
                neo.Image.SetPixel(0, 0, 0, 15, 0);
                neo.Update();

                WebServer server = new WebServer(80, HttpProtocol.Http);

                // Add a handler for commands that are received by the server.
                server.CommandReceived += ServerCommandReceived;

                // Start the server.
                server.Start();
            }
            

            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }

            private static void ServerCommandReceived(object source, WebServerEventArgs e)
            {
                var url = e.Context.Request.RawUrl;
                Debug.WriteLine($"Command received: {url}, Method: {e.Context.Request.HttpMethod}");

                if (url.ToLower() == "/sayhello")
                {
                WebServer.OutPutStream(e.Context.Response, "<html><head>" +
"<title>Hi from nanoFramework Server</title></head><body>You want me to say hello in a real HTML page!<br/><a href='/useinternal'>Generate an internal text.txt file</a><br />" +
"<a href='/Text.txt'>Download the Text.txt file</a><br>" +
"Try this url with parameters: <a href='/param.htm?param1=42&second=24&NAme=Ellerbach'>/param.htm?param1=42&second=24&NAme=Ellerbach</a></body></html>");
            }
                else
                {
                    WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
                }
            }
    }
}
