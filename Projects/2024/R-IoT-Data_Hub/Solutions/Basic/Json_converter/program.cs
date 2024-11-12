using System;
using System.Diagnostics;
using System.Threading;
//Demonštrácia konverzie veličín a ich hodnôt z dát. typu struct na Json formát a naopak (využiteľné pri LoRa prenosoch)
//funkcie nemajú skoro žiadne ošetrenie pred neočakávanými hodnotami, takže iné ako dobré vstupy spôsobia chyby/nepredvídateľné chovanie


namespace Json_LoRa_Msg
{

    public struct SensorData { public double Temperature; public double Humidity; }

    public static class SimpleJsonConverter
    {
        public static string SerializeObject(SensorData obj)
        {
            //if (obj ==null) { throw new ArgumentNullException(nameof(obj)); }
            var json = "{";
            var properties = obj.GetType().GetFields();

            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var name = property.Name;
                var value = property.GetValue(obj);
                json += "\"" + name + "\": " + value;
                if (i < properties.Length - 1) { json+=", "; }
            }
            json += "}";
            return json;
        }


        public static SensorData ParseJson(string json)
        {
            SensorData data = new SensorData();
            // Odstránenie zbytočných znakov
            json = json.Trim('{', '}');
            json = RemoveCharacters(json, new char[] { '\"' });// napr.  "temp" -> temp
            // Rozdelenie na jednotlivé páry kľúč-hodnota
            string[] pairs = json.Split(',');
            foreach (var pair in pairs)
            {
                string[] keyValue = pair.Split(':');
                string key = keyValue[0].Trim();
                string value = keyValue[1].Trim();
                // Priradenie hodnôt k vlastnostiam štruktúry
                if (key.Equals("Temperature"))
                {
                    data.Temperature = double.Parse(value);
                }
                else if (key.Equals("Humidity")) 
                {
                    data.Humidity = double.Parse(value); 
                }
            }
            return data;

        }

        public static string RemoveCharacters(string input, char[] charsToRemove) { 
            string result = "";
            foreach (char c in input) {
                bool shouldRemove = false; 
                foreach (char remove in charsToRemove) { 
                    if (c == remove) { shouldRemove = true; break; }
                }
                if (!shouldRemove) { result += c; }
            } return result; 
        }
    }
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

           
            // Vytvorenie objektu s nameranými hodnotami
            SensorData data = new SensorData { Temperature = 23.5, Humidity = 60.2 };
            // Konverzia objektu do formátu JSON
            string json = SimpleJsonConverter.SerializeObject(data);
            // Výpis JSON reťazca
            Console.WriteLine("We have imputed the SensorData struct, with the vals: temperature: " + data.Temperature + ", humidity: " + data.Humidity);
            Console.WriteLine("This is the Json it produced: ");
            Console.WriteLine(json);

            SensorData check_data = SimpleJsonConverter.ParseJson(json);
            Console.WriteLine("And now we have given the above Json, the following was extracted from it: temperature: " + check_data.Temperature + ", humidity: " + check_data.Humidity);
            


            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}
