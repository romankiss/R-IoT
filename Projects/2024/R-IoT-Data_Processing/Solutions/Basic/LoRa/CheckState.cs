using Iot.Device.Button;
using Iot.Device.Ssd13xx;
using nanoFramework.Hardware.Esp32;
using nanoFramework.Json;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

public class LampPhase
{
    public byte L1 { get; set; }
    public byte L2 { get; set; }
    public byte L3 { get; set; }
    public byte L4 { get; set; }
    public byte L5 { get; set; }
    public byte L6 { get; set; }
    public byte L7 { get; set; }
    public byte L8 { get; set; }
    public byte L9 { get; set; }
    public byte L10 { get; set; }
    public byte L11 { get; set; }
    public byte L12 { get; set; }
    public byte L13 { get; set; }
}
public class Phase
{
    public byte P1 { get; set; }
    public byte P2 { get; set; }
    public byte P3 { get; set; }
}

public class SystemData
{
    public int errmsg { get; set; }
    public int bat { get; set; }
}



namespace LoRa
{
    public class CheckState
    {


        public CheckState()
        {


        }
        public string SendData()
        {
            var lamp = new LampPhase { L1 = 1, L2 = 1, L4 = 1, L3 = 0, L5 = 0, L6 = 0, L7 = 0, L8 = 0, L9 = 0, L10 = 0, L11 = 0, L12 = 0, };
            var phase = new Phase { P1 = 0, P2 = 0, P3 = 1 };
            var sys = new SystemData { bat = 60, errmsg = 0 };

            string data = JsonConvert.SerializeObject(lamp).Trim('{', '}', '[', ']') + JsonConvert.SerializeObject(phase).Trim('{', '}', '[', ']') + JsonConvert.SerializeObject(sys).Trim('{', '}', '[', ']');
            data = ("{" + data + "}");
            return data;
        }
    }

}
