using System;
using System.Diagnostics;
using System.Threading;

namespace Json_LoRa_Msg
{
    //create a struct that has public fields so that it can be converted to json
    public struct SensorData { public double Temperature; public double Humidity; }

    public static class SimpleJsonConverter
    {
        public static string SerializeObject(SensorData obj)
        {
            //if (obj ==null) { throw new ArgumentNullException(nameof(obj)); }
            //every valid json starts with { and ends with }
            var json = "{";
            var properties = obj.GetType().GetFields();//creates a list of the properties / fields of the given struct/class/object

            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var name = property.Name;//gets the name
                var value = property.GetValue(obj);//gets the val
                json += "\"" + name + "\": " + value;//formats the name and its value to a json format
                if (i < properties.Length - 1) { json+=", "; }//if there will be some more fields a ", " is added
            }
            json += "}";
            return json;
        }

    }
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

           
            // Vytvorenie objektu s nameranými hodnotami
            SensorData data = new SensorData { Temperature = 23.5, Humidity = 60.2 };//creates an object from the struct and assigns some values to its properties
            // Konverzia objektu do formátu JSON
            string json = SimpleJsonConverter.SerializeObject(data);//converts the above created obj to a json format of a string
            // Výpis JSON reťazca
            Console.WriteLine(json);


            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}
