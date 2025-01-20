// written by matohappy, January 20, 2025
// version: 1.0.0.0  01/20/2025  beta version


using nanoFramework.Hardware.Esp32;
using System;
using System.Device.Gpio;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Device.I2c;
using nanoFramework.Json;
using Memory = nanoFramework.Runtime.Native.GC;

namespace LoRaE22PhaseChecker
{
    public class Program
    {
        //LoRa e22
        static int loopback_counter = 0;
        static E22 lora = null;
        static ushort broadcastAddress = 0xFFFF;
        const ushort loraAddress = 0x1926;  //55;
        const byte loraNetworkId = 0x12;    // 850.125 + 18 = 868.125Mhz

        //Phase checker
        static int rvo = 1;                                                 //ID number of rvo
        static int clockPin = 0;                                            //Clock phase pin
        static int[] phasePins = { 16, 17, 18 };                            //Phase pins
        static int[] lampPhasePins = { 4, 5, 6, 8, 9, 12, 13, 14, 15, 21 }; //Lamp phase pins

        static string js = null;
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");



            
            #region LoRa
            Configuration.SetPinFunction(41, DeviceFunction.COM2_TX);   //ESP32 S3 Atom 1
            Configuration.SetPinFunction(42, DeviceFunction.COM2_RX);   //ESP32 S3 Atom 2

            lora = E22.Create("COM2", loraAddress: loraAddress);
            if (lora != null)
            {
                lora.OnPacketReceived += (sender, e) =>
                {
                    Debug.WriteLine($"nFmem_LoRaRcv={Memory.Run(true)}");
                    var rcvData = e.Data;
                    Debug.WriteLine(e.Data.ToString());

                };
                //sensors.Append("LoRa ");
            }
            #endregion
            
            var Pins = new PinsValues(lampPhasePins, phasePins, clockPin, rvo);


            js = Pins.PinVal();
            Debug.Write($"len={js.Length}, {js}");
            Debug.Write("?");


            Timer time = new Timer(Send, null, 20000, 20000);

            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
        public static void Send(object state)
        {
            lora.Send(65535, js);
        }

    }

}
