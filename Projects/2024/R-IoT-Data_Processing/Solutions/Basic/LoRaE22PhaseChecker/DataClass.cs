using nanoFramework.Json;
using System;
using System.Text;

namespace LoRaE22PhaseChecker
{
    public class Rvo
    {
        public int P { get; set; }           //Phase
        public int PC { get; set; }            //Phase clock
        public int LP { get; set; }          //Lamp phase
        //public DateTime LU { get; set; }        //Last Update
        public SystemData SD { get; set; }      //System Data
        public Sensors S { get; set; }          //Sensors

        public string GetData()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class SystemData
    {
        public int R { get; set; }              //rvo 
        //public string location { get; set; }
        //public int lora { get; set; }           //LoRa ID
        public int err { get; set; }            //error messages
        public int bat { get; set; }            //battery 
    }

    public class Sensors
    {
        public byte H { get; set; }            //Humidity
        public short T { get; set; }            //Temperature
    }
}
