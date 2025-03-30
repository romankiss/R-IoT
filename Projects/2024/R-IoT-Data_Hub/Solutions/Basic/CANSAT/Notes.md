
<h3>Notes</h3>
1. Treba doplnit v dokumentacii (Formát LoRa správy), ze sprava (payload) je ukonceny CRLF (0x0D, 0x0A) 
</br>
 example:       <b2> #123T25.55H48.12P998D10\r\n </b2>     
</br>
</br>
2. Vysielanie telemetry data ma byt kazdu secundu, takze
</br>
const int pub_period = <b>1000</b>;     // miliseconds
</br>
</br>
pre lepsiu spolahlivost pri tejto frekvencie vysielania je lepsie pouzit SendAsync prikaz, takze
</br>
lora.<b>SendAsync</b>(address: BroadcastAddress, data: payload);
</br>
</br>
Pre zistenie kolko je free memory a spustenie GC (garbage collection) sa pouziva call
</br>
Memory.Run(true)
</br>
Pre zistenie kolko je len free memory sa pouziva call
</br>
Memory.Run(false)
</br>
takze je dobre mat prehlad o poziti nF memory, ci nie je nejaky leak, atd. na zaciatku Fire a pri jeho ukonceni.
</br>
</br>
3. Tips for GroundStation code 
</br>
<pre>
// extracting a telemetry data
// sample
string input = "#123T22.33H10.05P0.12345D-1\r\n"; 
// creating a telemetryData dictionary
var values = Regex.Split(input, @"([#\r\nA-Z]+)").Where(s => s != String.Empty && s != "\r\n").ToArray();
var telemetryData = Enumerable.Range(0, values.Length / 2).ToDictionary(i => values[2 * i], i => values[2 * i + 1]);
// usage
float.TryParse(telemetryData["T"], out float temperature);
var distance = telemetryData["D"];
// simple csv record
string csv = string.Join(",", telemetryData.Values) + "\r\n";
</pre>

4. Adding GPS Reader 
</br>
The following link shows a support for UART GPS (M5Stack, E108 GN02D, GN03D, etc.) Reader with a GGA parser:
</br>
https://github.com/romankiss/R-IoT/tree/d16b5e20ffb81b1302e4cf6bcd1e4b18cde1d2d9/Projects/2024/PatternsAndPractices/GPS
</br>
4a. The following is the connection pins for LoRa and GPS on the Atom Motion (working test):
<pre>
const int pinI2C1_SDA = 2;      // Grove
const int pinI2C1_SCK = 1;      // Grove
const int pinCOM2_TX = 6;       // PORTC-G6, LoRa 
const int pinCOM2_RX = 5;       // PORTC-G5,      
const int pinCOM1_TX = 8;       // PORTB-G8, GPS
const int pinCOM1_RX = 7;       // PORTB-G7
</pre>
</br>
4b. GPS output during the cold start and after:
</br>
<pre>
 $GNGGA,070814.000,,,,,0,00,25.5,,,,,,*70
 $GNGGA,070815.998,4836.05441,N,01750.18459,E,1,07,3.2,205.9,M,0.0,M,,*76
</pre>
</br>
4c. Usage of the GPS for CanSat project, see the following link:
</br>
https://github.com/romankiss/R-IoT/blob/main/Projects/2024/PatternsAndPractices/GPS/usage.cs
</br>
Note, that the serialport receiver has a resposibility only to cache a GGA raw data in the threadsafe manner, without their parsing. The parsing of the GGA raw cached data is threadsafe during its publishing and after that they are clean-up for next data, in other words, the location is zero.









