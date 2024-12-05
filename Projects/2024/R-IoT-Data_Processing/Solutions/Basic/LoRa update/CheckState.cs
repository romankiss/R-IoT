using Iot.Device.Button;
using Iot.Device.Ssd13xx;
using nanoFramework.Hardware.Esp32;
using nanoFramework.Json;
using System.Device.Gpio;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

public class SystemData
{
    public int errmsg { get; set; }
    public int bat { get; set; }
}



namespace LoRa
{

    public class CheckState2
    {
        public int id { get; set; }
        public SystemData SysData { get; set; }
        public int LPh { get; set; }
        public int Ph { get; set; }

        public CheckState2()
        {
        }
        public string SendData()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    


}
