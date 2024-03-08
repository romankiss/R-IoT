using nanoFramework.WebServer;
using System;
using System.Diagnostics;

using System.Net;

//using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace websajt
{
    internal class Server
    {
        static string file_path = null;
        public static void start(string telemetry_data_file_path)
        {
            file_path = telemetry_data_file_path;
            WebServer server = new WebServer(80, HttpProtocol.Http);

            // Add a handler for commands that are received by the server.
            server.CommandReceived += ServerCommandReceived;

            // Start the server.
            server.Start();
        }
        private static void ServerCommandReceived(object source, WebServerEventArgs e)
        {
            var url = e.Context.Request.RawUrl;
            Debug.WriteLine($"Command received: {url}, Method: {e.Context.Request.HttpMethod}");

            if (url.ToLower() == "/sayhello")
            {
                WebServer.OutPutStream(e.Context.Response,
                    "<!DOCTYPE html> <html><head>" +
                    "<title>Hi from nanoFramework Server</title></head>" +
                    "<body>THIS IS A WEB PAGE!</body></html>");
                Blink.set(0, 15, 0, 1, 1.0, 1);
            }
            else if (url.ToLower() == "/telemetry_data")
            {
                WebServer.OutPutStream(e.Context.Response,
                    "<!DOCTYPE html> <html><head>" +
                    "<title>Hi from nanoFramework Server</title></head>" +
                    "<body>Telemetry data records: <br>" +
                    $"{websajt.Storage.read(file_path)}" +
                    "</body></html>"
                    );
                Blink.set(0, 15, 0, 1, 1.0, 1);
            }

            else
            {
                WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
                Blink.set(15, 0, 0, 500, 1);
            }
        }
    }
}
