namespace LoRa
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
                if (i < properties.Length - 1) { json += ", "; }
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

        public static string RemoveCharacters(string input, char[] charsToRemove)
        {
            string result = "";
            foreach (char c in input)
            {
                bool shouldRemove = false;
                foreach (char remove in charsToRemove)
                {
                    if (c == remove) { shouldRemove = true; break; }
                }
                if (!shouldRemove) { result += c; }
            }
            return result;
        }
    }

}
