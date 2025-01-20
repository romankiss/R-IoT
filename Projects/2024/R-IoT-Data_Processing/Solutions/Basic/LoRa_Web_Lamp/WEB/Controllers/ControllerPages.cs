// written by matohappy, January 20, 2025
// version: 1.0.0.0  01/20/2025  beta version

using nanoFramework.Networking;
using nanoFramework.WebServer;
using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace LoRa_Web_Lamp.WEB.Controllers
{
    public class ControllerPages
    {
        private static string _ipadd;
        private static int _rvoid = -1;
        public ControllerPages(string ipadd)
        {
            _ipadd = ipadd;
        }

        public static void RvoId(int rvoid)
        {
            _rvoid = rvoid;
        }


        [Route("/")]
        public void RoutePostTest(WebServerEventArgs e)
        {

            WebServer.OutPutStream(e.Context.Response, "<!DOCTYPE html>\r\n" +
"<html lang=\"en\">\r\n" +
"<head>\r\n" +
"<meta charset=\"UTF-8\">\r\n" +
"<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n" +
"<title>Riadna Lampa</title>\r\n" +
"</head>\r\n" +
            #region style
"<style>\r\n" +
"ul{\r\n" +                             //ul
"list-style-type: none;\r\n" +
"margin: 0;\r\n" +
"padding: 0;\r\n" +
"overflow: hidden;\r\n" +
"background-color: #333;\r\n" +
"}\r\n" +

"li{\r\n" +                             //li
"float: left;\r\n" +
"}\r\n" +

"li, a{\r\n" +                          //li a
"display: block;\r\n" +
"color: white;\r\n" +
"text-align: center;\r\n" +
"padding: 14px 16px;\r\n" +
"text-decoration: none;\r\n" +
"}\r\n" +

"li:hover, a:hover{\r\n" +              //li a
"background-color: #111;\r\n" +
"}\r\n" +

".active{\r\n" +
"background-color: #04AA6D;\r\n" +
"}\r\n" +

"ul{\r\n" +                             //ul
"position: fixed;\r\n" +
"top: 0;\r\n" +
"width: 100%;\r\n" +
"}\r\n" +
"</style>\r\n" +
            #endregion
            #region body
"<body>\r\n" +
"<ul>\r\n" +                            //nav
"<li><a href=\"/\">Home</a></li>\r\n" +
"<li><a href=\"/switchboards\">Switchboards</a></li>\r\n" +
"<li><a href=\"buttons\">Nothing Now</a></li>\r\n" +
"<li><a href=\"\">About</a></li>\r\n" +
"</ul>\r\n" +
"</body>\r\n" +
            #endregion
"</html");
        }
        [Route("/buttons")]
        public void RoutePostButtons(WebServerEventArgs e)
        {

            WebServer.OutPutStream(e.Context.Response, "<!DOCTYPE html>\r\n" +
"<html lang=\"en\">\r\n" +
"<head>\r\n" +
"<meta charset=\"UTF-8\">\r\n" +
"<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n" +
"<title>Riadna Lampa</title>\r\n" +
"</head>\r\n" +
            #region style
"<style>\r\n" +
"ul{\r\n" +                             //ul
"list-style-type: none;\r\n" +
"margin: 0;\r\n" +
"padding: 0;\r\n" +
"overflow: hidden;\r\n" +
"background-color: #333;\r\n" +
"}\r\n" +

"li{\r\n" +                             //li
"float: left;\r\n" +
"}\r\n" +

"li, a{\r\n" +                          //li a
"display: block;\r\n" +
"color: white;\r\n" +
"text-align: center;\r\n" +
"padding: 14px 16px;\r\n" +
"text-decoration: none;\r\n" +
"}\r\n" +

"li:hover, a:hover{\r\n" +              //li a
"background-color: #111;\r\n" +
"}\r\n" +

".active{\r\n" +
"background-color: #04AA6D;\r\n" +
"}\r\n" +

"ul{\r\n" +                             //ul
"position: fixed;\r\n" +
"top: 0;\r\n" +
"width: 100%;\r\n" +
"}\r\n" +
"</style>\r\n" +
            #endregion
            #region script
"<script>\r\n" +
"async function TurnOn() {\r\n" +       //post
"const ip = document.getElementById('adresa').value;\r\n" +
"const but = document.getElementById('but').value;\r\n" +
"const url = `http://${ip}/api/button`;\r\n\r\n" +
"try {\r\n" +
"const response = await fetch(url, {\r\n" +
"method: 'POST',\r\n" +
"body: but\r\n" +
"});\r\n\r\n" +
"if (response.ok) {\r\n" +
"const result = await response.text();\r\n" +
"alert('Dakujeme tu mas test: ' + result);\r\n" +
"return;\r\n" +
"}\r\n\r\n" +
"alert('Error: ' + response.statusText);\r\n" +
"} \r\n" +
"catch (error) {\r\n" +
"alert('Error: ' + error.message);\r\n" +
"}\r\n" +
"}\r\n" +
"async function TurnOff() {\r\n" +      //post
"const ip = document.getElementById('adresa').value;\r\n" +
"const but = document.getElementById('but2').value;\r\n" +
"const url = `http://${ip}/api/button`;\r\n\r\n" +
"try {\r\n" +
"const response = await fetch(url, {\r\n" +
"method: 'POST',\r\n" +
"body: but\r\n" +
"});\r\n\r\n" +
"if (response.ok) {\r\n" +
"const result = await response.text();\r\n" +
"alert('Dakujeme tu mas test: ' + result);\r\n" +
"return;\r\n" +
"}\r\n\r\n" +
"alert('Error: ' + response.statusText);\r\n" +
"} \r\n" +
"catch (error) {\r\n" +
"alert('Error: ' + error.message);\r\n" +
"}\r\n" +
"}\r\n" +
"</script>\r\n" +
            #endregion
            #region body
"<body>\r\n" +
"<ul>\r\n" +                            //nav
"<li><a href=\"/\">Home</a></li>\r\n" +
"<li><a href=\"/switchboards\">Switchboards</a></li>\r\n" +
"<li><a href=\"/buttons\">Nothing Now</a></li>\r\n" +
"<li><a href=\"\">About</a></li>\r\n" +
"</ul>\r\n" +
"<h1>Posli Spravu</h1>\r\n" +           //test
"<label for=\"adresa\">IPcka:</label>\r\n" +
"<input type=\"text\" id=\"adresa\" name=\"adresa\" value=\"" + _ipadd + "\">\r\n" +
"<br><br>\r\n" +
"<button name=\"but\" id=\"but\" value=\"OnRele\" onclick=\"TurnOn()\">On rele</button>\r\n" +
"<br><br>\r\n" +
"<button name=\"but2\" id=\"but2\" value=\"OffRele\" onclick=\"TurnOff()\">Off rele</button>\r\n" +
"</body>\r\n" +
            #endregion
"</html>");
        }
        [Route("/switchboards")]
        public void Switchboards(WebServerEventArgs e)
        {
            string json = System.IO.File.ReadAllText("D:\\MainRvoData.json");
            string[] sub = json.Split('\n');



            WebServer.OutPutStream(e.Context.Response, "<!DOCTYPE html>\r\n" +
"<html lang=\"en\">\r\n" +
"<head>\r\n" +
"<meta charset=\"UTF-8\">\r\n" +
"<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n" +
"<title>Riadna Lampa</title>\r\n" +
"</head>\r\n" +
            #region style
"<style>\r\n" +
"table {\r\n" +                         //table
"font-family: arial, sans-serif;\r\n" +
"border-collapse: collapse;\r\n" +
"width: 100%;\r\n" +
"margin-top: 100px;\r\n" +
"}\r\n" +

"td, th {\r\n" +                        //td th
"border: 1px solid #dddddd;\r\n" +
"text-align: left;\r\n" +
"padding: 8px;\r\n" +
"}\r\n" +

"tr: nth-child(even) {\r\n" +           //tr
"background-color: #dddddd;\r\n" +
"}\r\n" +

"ul{\r\n" +                             //ul
"list-style-type: none;\r\n" +
"margin: 0;\r\n" +
"padding: 0;\r\n" +
"overflow: hidden;\r\n" +
"background-color: #333;\r\n" +
"}\r\n" +

"li{\r\n" +                             //li
"float: left;\r\n" +
"}\r\n" +

"li, a{\r\n" +                          //li a
"display: block;\r\n" +
"color: white;\r\n" +
"text-align: center;\r\n" +
"padding: 14px 16px;\r\n" +
"text-decoration: none;\r\n" +
"}\r\n" +

"li:hover, a:hover{\r\n" +              //li a
"background-color: #111;\r\n" +
"}\r\n" +

".active{\r\n" +    
"background-color: #04AA6D;\r\n" +
"}\r\n" +

"ul{\r\n" +                             //ul
"position: fixed;\r\n" +
"top: 0;\r\n" +
"width: 100%;\r\n" +
"}\r\n" +
"</style>\r\n" +
            #endregion
            #region body
"<body>\r\n" +
"<ul>\r\n" +
"<li><a href=\"/\">Home</a></li>\r\n" +
"<li><a href=\"/switchboards\">Switchboards</a></li>\r\n" +
"<li><a href=\"/buttons\">Nothing Now</a></li>\r\n" +
"<li><a href=\"\">About</a></li>\r\n" +
"</ul>\r\n" +
"<table>\r\n" +
"<tbody id = \"container\">\r\n" +
"<tr>\r\n" +
"<th> RVO number </th>\r\n" +
"<th> RVO location </th>\r\n" +
"<th> LoRa number </th>\r\n" +
"<th> Phase1 </th>\r\n" +
"<th> Phase2 </th>\r\n" +
"<th> Phase3 </th>\r\n" +
"<th> Phase clock </th>\r\n" +
"<th> Last update </th>\r\n" +
"</tr>\r\n" +
"</tbody>\r\n" +
"</table>\r\n" +
"</body>\r\n" +
            #endregion
            #region script
"<script>\r\n" +
"async function postRvoValue(id)\r\n" +
"{\r\n" +

"document.getElementById(id);\r\n" +
"const url = `http://"+_ipadd+"/api/switchboards`;\r\n" +
"try\r\n" +
"{\r\n" +

"const response = await fetch(url, {\r\n" +
"method: 'POST',\r\n" +
"body: id\r\n" +
"});\r\n" +
"if (response.ok)\r\n" +
"{\r\n" +
"const result = await response.text();\r\n" +
"location.href = \"/switchboards/lampphases?rvo= \" + id;\r\n" +
"return;\r\n" +
"}\r\n" +
"alert('Error: ' + response.statusText);\r\n" +
"}\r\n" +
"catch (error)\r\n" +
"{\r\n" +
"alert('Error: ' + error.message);\r\n" +
"}\r\n" +
"}\r\n" +
"const lamps = [" + json + "]\r\n" +

"const container = document.getElementById('container');\r\n" +             //container

"for (let i = 0; i < lamps.length; i++){\r\n" +
"const row = document.createElement(\"tr\");\r\n" +
"const lamp = lamps[i];\r\n" +

"const rvo = document.createElement(\"td\");\r\n" +                         //Rvo ID
"rvo.innerText = `${ lamp.SD.R}`;\r\n" +
"row.appendChild(rvo);\r\n" +

"const location = document.createElement(\"td\");\r\n" +                    //Rvo location
"location.innerText = `${ lamp.SD.location}`;\r\n" +
"row.appendChild(location);\r\n" +

"const loraNum = document.createElement(\"td\");\r\n" +                     //Lora address
"loraNum.innerText = `${ lamp.SD.lora}`;\r\n" +
"row.appendChild(loraNum);\r\n" +

"for (const phase of lamp.P) {\r\n" +                                       //Phases
"const phaseElement = document.createElement(\"td\");\r\n" +
"phaseElement.innerText = phase ? \"ON\" : \"OFF\";\r\n" +
"row.appendChild(phaseElement);\r\n" +
"}\r\n" +

"const phaseClock = document.createElement(\"td\");\r\n" +                  //Phase clock
"phaseClock.innerText = lamp.PC ? \"ON\" : \"OFF\";\r\n" +
"row.appendChild(phaseClock);\r\n" +

"const lastUpdate = document.createElement(\"td\");\r\n" +                  //Last update
"lastUpdate.innerText = `${ lamp.LU}`;\r\n" +
"row.appendChild(lastUpdate);\r\n" +

"const rows = document.createElement(\"td\");\r\n" +                        //Button
"rows.innerHTML = `<button id='${lamp.SD.R}' onclick='postRvoValue(id)'>Details</button>`\r\n" +
"row.appendChild(rows);\r\n" +

"container.appendChild(row);\r\n" +
"}\r\n" +
"</script>\r\n" +
            #endregion
"</html>");
        }


        [Route("/switchboards/lampphases")]
        public void LampPhases(WebServerEventArgs e)
        {
            string json = System.IO.File.ReadAllText("D:\\Rvo"+_rvoid+".json");
            Debug.WriteLine("System.IO.File.ReadAllText(I://telemetryData" + _rvoid + ".json)");
            string[] sub = json.Split('\n');

            WebServer.OutPutStream(e.Context.Response, "<!DOCTYPE html>\r\n" +
"<html lang=\"en\">\r\n" +
"<head>\r\n" +
"<meta charset=\"UTF-8\">\r\n" +
"<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n" +
"<title>Riadna Lampa</title>\r\n" +
"</head>\r\n" +
            #region style
"<style>\r\n" +
"table {\r\n" +                         //table
"font-family: arial, sans-serif;\r\n" +
"border-collapse: collapse;\r\n" +
"width: 100%;\r\n" +
"margin-top: 100px;\r\n" +
"}\r\n" +

"td, th {\r\n" +                        //td th
"border: 1px solid #dddddd;\r\n" +
"text-align: left;\r\n" +
"padding: 8px;\r\n" +
"}\r\n" +

"tr: nth-child(even) {\r\n" +           //tr
"background-color: #dddddd;\r\n" +
"}\r\n" +

"ul{\r\n" +                             //ul
"list-style-type: none;\r\n" +
"margin: 0;\r\n" +
"padding: 0;\r\n" +
"overflow: hidden;\r\n" +
"background-color: #333;\r\n" +
"}\r\n" +

"li{\r\n" +                             //li
"float: left;\r\n" +
"}\r\n" +

"li, a{\r\n" +                          //li a
"display: block;\r\n" +
"color: white;\r\n" +
"text-align: center;\r\n" +
"padding: 14px 16px;\r\n" +
"text-decoration: none;\r\n" +
"}\r\n" +

"li:hover, a:hover{\r\n" +              //li a
"background-color: #111;\r\n" +
"}\r\n" +

".active{\r\n" +
"background-color: #04AA6D;\r\n" +
"}\r\n" +

"ul{\r\n" +                             //ul
"position: fixed;\r\n" +
"top: 0;\r\n" +
"width: 100%;\r\n" +
"}\r\n" +
"</style>\r\n" +
            #endregion
            #region body
"<body>\r\n" +
"<ul>\r\n" +
"<li><a href=\"/\">Home</a></li>\r\n" +                                     //nav
"<li><a href=\"/switchboards\">Switchboards</a></li>\r\n" +
"<li><a href=\"/buttons\">Nothing Now</a></li>\r\n" +
"<li><a href=\"\">About</a></li>\r\n" +
"</ul>\r\n" +
"<table>\r\n" +
"<tr>\r\n" +
"<th> RVO number </th>\r\n" +
"<th> RVO location </th>\r\n" +
"<th> LoRa ID </th>\r\n" +
"<th> Phase clock </th>\r\n" +
"<th> Last update </th>\r\n" +
"</tr>\r\n" +
"<tbody id = \"rvoDetais\">\r\n" +
"</tbody>\r\n" +
"</table>\r\n" +
"<table>\r\n" +
"<tbody id = \"phasetable\">\r\n" +
"</tbody>\r\n" +
"<tbody id = \"container\">\r\n" +
"</tbody>\r\n" +
"</table>\r\n" +
"</body>\r\n" +
            #endregion
            #region script
"<script>\r\n" +
"const lamps = [" + json +"]\r\n" +
"const url = new URL(window.location.href);\r\n" +
"const params = url.searchParams.get(\"rvo\");\r\n" +

"const phasetable = document.getElementById(\"phasetable\");\r\n" +         //phasetable

"const phtb = document.createElement(\"tr\");\r\n" +
"for (let i = 0; i < 10; ++i){\r\n" +
"const phtab = document.createElement(\"th\");\r\n" +
"phtab.innerText = \"phase\" + i;\r\n" +
"phtb.appendChild(phtab);\r\n" +
"}\r\n" +

"const phaseClock = document.createElement(\"th\");\r\n" +
"phaseClock.innerText = \"PhaseClock\";\r\n" +
"phtb.appendChild(phaseClock);\r\n" +

"const phase = document.createElement(\"th\");\r\n" +
"phase.innerText = \"Phase\";\r\n" +
"phtb.appendChild(phase);\r\n" +

"const LastUpdate = document.createElement(\"th\");\r\n" +
"LastUpdate.innerText = \"LastUpdate\";\r\n" +
"phtb.appendChild(LastUpdate);\r\n" +
"phasetable.appendChild(phtb);\r\n" +

"const rvoDetais = document.getElementById(\"rvoDetais\");\r\n" +           //rvoDetails

"for (let i = 0; i < lamps.length; i++){\r\n" +
"const lamp = lamps[i]\r\n" +
"const row = document.createElement(\"tr\");\r\n" +

"const rvo = document.createElement(\"td\");\r\n" +                         //Rvo ID
"rvo.innerText = `${ lamp.R}`;\r\n" +
"row.appendChild(rvo);\r\n" +

"const location = document.createElement(\"td\");\r\n" +                    //Rvo location
"location.innerText = `${ lamp.SD.location}`;\r\n" +
"row.appendChild(location);\r\n" +

"const LoRa = document.createElement(\"td\");\r\n" +                        //Lora address
"LoRa.innerText = `${ lamp.SD.lora}`;\r\n" +
"row.appendChild(LoRa);\r\n" +

"const phaseClock = document.createElement(\"td\");\r\n" +                  //Phase clock
"phaseClock.innerText = lamp.PC ? \"ON\" : \"OFF\";\r\n" +
"row.appendChild(phaseClock);\r\n" +

"const lastUpdate = document.createElement(\"td\");\r\n" +                  //Last update
"lastUpdate.innerText = `${ lamp.LU}`;\r\n" +
"row.appendChild(lastUpdate);\r\n" +

"rvoDetais.appendChild(row);\r\n" +
"break;\r\n" +
"}\r\n" +

"const container = document.getElementById('container');\r\n" +             //container

"for (let i = 0; i<lamps.length; i++) {\r\n" +
"const lamp = lamps[i]\r\n" +
"const row = document.createElement(\"tr\");\r\n" +

"for (const phases of lamp.LP) {\r\n" +                                     //Lamp phases
"const phasesElement = document.createElement(\"td\");\r\n" +
"phasesElement.innerText = phases? \"ON\" : \"OFF\";\r\n" +
"row.appendChild(phasesElement);\r\n" +
"}\r\n" +

"const phaseClock = document.createElement(\"td\");\r\n" +                  //Phase clock
"phaseClock.innerText = lamp.PC ? \"ON\" : \"OFF\";\r\n" +
"row.appendChild(phaseClock);\r\n" +

"const Phase = document.createElement(\"td\");\r\n" +                       //Phases
"let x = 0;\r\n" +
"for (const phases of lamp.P) {\r\n" +
"if(phases == true) {++x;}\r\n" +
"}\r\n" +
"Phase.innerText = x + \"/ \" + lamp.P.length;\r\n" +
"row.appendChild(Phase);\r\n" +

"const lastUpdate = document.createElement(\"td\");\r\n" +                  //Last update
"lastUpdate.innerText = lamp.LU;\r\n" +
"row.appendChild(lastUpdate);\r\n" +

"container.appendChild(row);\r\n" +
"}\r\n" +
"</script>\r\n" +
            #endregion
"</html>");
        }
    }
}
