using System;
using System.Diagnostics;
using System.Threading;
using nanoFramework.Json;

namespace NFAppESP32C3_Testing
{
    public class SensorData 
    { 
        public double Temp { get; set; } 
        public double Hum { get; set; } 
    }


    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            // publisher
            SensorData data = new SensorData { Temp = 23.5, Hum = 60.2 };
            string jsontext = JsonConvert.SerializeObject(data);
            Debug.WriteLine($"data= {jsontext}");

          
            // receiver 1
            SensorData rcvData = JsonConvert.DeserializeObject(jsontext, typeof(SensorData)) as SensorData;
            jsontext = JsonConvert.SerializeObject(rcvData);
            Debug.WriteLine($"data= {jsontext}");



            // receiver 2
            string jsontext3 = "{\"Hum\":65.00, \"Id\":1234567890, \"Temp\":23.50, \"Desc\":\"nothing\" }";
            SensorData rcvData2 = JsonConvert.DeserializeObject(jsontext3, typeof(SensorData)) as SensorData;
            jsontext = JsonConvert.SerializeObject(rcvData2);
            Debug.WriteLine($"data= {jsontext}");

            Thread.Sleep(Timeout.Infinite);

            
        }
    }
}
