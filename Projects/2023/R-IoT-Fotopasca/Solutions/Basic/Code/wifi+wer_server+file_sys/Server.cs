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
        public static  void start() 
        {
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
                Blink.set(0, 15, 0, 500, 2);
            }

            else
            {
                WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.NotFound);
                Blink.set(15, 0, 0, 500, 2);
            }
        }
    }
}
