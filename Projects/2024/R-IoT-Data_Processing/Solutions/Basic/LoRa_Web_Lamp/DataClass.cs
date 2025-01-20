// written by matohappy, January 20, 2025
// version: 1.0.0.0  01/20/2025  beta version

using nanoFramework.Json;
using System;
using System.Text;

namespace LoRa_Web_Lamp
{
    public class Rvo
    {
        public int P { get; set; }           //Phase
        public int PC { get; set; }            //Phase clock
        public int LP { get; set; }          //Lamp phase
        public DateTime LU { get; set; }        //Last Update
        public SystemData SD { get; set; }      //System Data
        public Sensors S { get; set; }          //Sensors
    }

    public class SystemData
    {
        public int R { get; set; }              //rvo 
        public string location { get; set; }    //Physic location of rvo
        public int lora { get; set; }           //LoRa ID
        public int err { get; set; }            //error messages
        public int bat { get; set; }            //battery 
    }

    public class Sensors
    {
        public byte H { get; set; }            //Humidity
        public short T { get; set; }            //Temperature
    }

    public class Lamp
    {
        public bool[] P { get; set; }           //Phase
        public bool PC { get; set; }            //Phase clock
        public bool[] LP { get; set; }          //Lamp phase
        public string LU { get; set; }        //Last Update
        public SystemData SD { get; set; }      //System Data
        public Sensors S { get; set; }          //Sensors

    }
