using LoRa_Web_Lamp.WEB.Controllers;
using nanoFramework.WebServer;
using System;
using System.Diagnostics;
using System.Text;

namespace LoRa_Web_Lamp.WEB
{
    internal class WebService : IDisposable
    {
        private WebServer _server;

        public WebService()
        {
            _server = new WebServer(80, HttpProtocol.Http, new Type[] { typeof(ControllerAPI), typeof(ControllerPages) });
            _server.SslProtocols = System.Net.Security.SslProtocols.Tls12;
            _server.Start();
            Debug.WriteLine("Started web");
        }

        public void Dispose()
        {
            _server.Dispose();
        }

    }
}
