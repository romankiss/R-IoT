// written by matohappy, January 20, 2025
// version: 1.0.0.0  01/20/2025  beta version

using nanoFramework.WebServer;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace LoRa_Web_Lamp.WEB.Controllers
{
    public class ControllerAPI
    {
        [Route("api/time")]
        [Method("POST")]
        public void RoutePostTest(WebServerEventArgs e)
        {
            string requestBody = null;
            try
            {
                byte[] buffer = new byte[e.Context.Request.ContentLength64];
                e.Context.Request.InputStream.Read(buffer, 0, buffer.Length);
                requestBody = UTF8Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            if (requestBody is null)
            {
                WebServer.OutPutStream(e.Context.Response, "Something went wrong");
                return;
            }

            Debug.WriteLine(requestBody);

            WebServer.OutPutStream(e.Context.Response, $"This is your body: {requestBody}");

        }



        [Route("api/button")]
        [Method("POST")]
        public void RoutePostButtons(WebServerEventArgs e)
        {
            string requestBody = null;
            try
            {
                byte[] buffer = new byte[e.Context.Request.ContentLength64];
                e.Context.Request.InputStream.Read(buffer, 0, buffer.Length);
                requestBody = UTF8Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            if (requestBody is null)
            {
                WebServer.OutPutStream(e.Context.Response, "Something went wrong");
                return;
            }
            Debug.WriteLine(requestBody);

            WebServer.OutPutStream(e.Context.Response, $"This is your body: {requestBody}");

        }
        [Route("api/switchboards")]
        [Method("POST")]
        public void RoutePostRvoID(WebServerEventArgs e)
        {
            string requestBody = null;
            try
            {
                byte[] buffer = new byte[e.Context.Request.ContentLength64];
                e.Context.Request.InputStream.Read(buffer, 0, buffer.Length);
                requestBody = UTF8Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            if (requestBody is null)
            {
                WebServer.OutPutStream(e.Context.Response, "Something went wrong");
                return;
            }

            ControllerPages.RvoId(Int16.Parse(requestBody));

            Debug.WriteLine(requestBody);

            WebServer.OutPutStream(e.Context.Response, $"This is your body: {requestBody}");

        }

    }

}
