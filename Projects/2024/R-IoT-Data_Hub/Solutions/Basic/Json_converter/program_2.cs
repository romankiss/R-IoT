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

          
            // receiver
            SensorData rcvData = JsonConvert.DeserializeObject(jsontext, typeof(SensorData)) as SensorData;
            string jsontext2 = JsonConvert.SerializeObject(rcvData);
            Debug.WriteLine($"data= {jsontext2}");


            Thread.Sleep(Timeout.Infinite);

            
        }
    }
}
