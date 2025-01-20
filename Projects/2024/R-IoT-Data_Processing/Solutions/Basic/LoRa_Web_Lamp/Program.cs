// written by matohappy, January 20, 2025
// version: 1.0.0.0  01/20/2025  beta version

using nanoFramework.Hardware.Esp32;
using System;
using System.Diagnostics;
using System.Threading;
using System.Device.Gpio;
using Memory = nanoFramework.Runtime.Native.GC;
using nanoFramework.Json;
using nanoFramework.System.IO.FileSystem;
using System.Text;
using System.IO;

namespace LoRa_Web_Lamp
{

    public class Program
    {
        //LoRa
        static int loopback_counter = 0;
        static E22 lora = null;
        static ushort broadcastAddress = 0xFFFF;
        const ushort loraAddress = 0x0533;  //55;
        const byte loraNetworkId = 0x12;    // 850.125 + 18 = 868.125Mhz

        public static void Main()
        {
            #region Wifi
            const string ssid = ""; //SSID
            const string password = ""; //Password

            Wifi wifi = new Wifi(ssid, password);
            #endregion

            #region SD Card Storage
            // E = USB storage
            // D = SD Card
            // I = Internal storage
            const string DirectoryPath = "D:\\";
            const string telemetrydataFilePath = DirectoryPath + "Rvo";
            Configuration.SetPinFunction(14, DeviceFunction.SPI2_MISO);
            Configuration.SetPinFunction(12, DeviceFunction.SPI2_CLOCK);
            Configuration.SetPinFunction(13, DeviceFunction.SPI2_MOSI);
            var mycard0 = new SDCard(new SDCardSpiParameters { spiBus = 2, chipSelectPin = 11 });
            var storage = new WebStorage(telemetrydataFilePath,mycard0);
            #endregion


            string[] listFiles = Directory.GetFiles(DirectoryPath);
            Debug.WriteLine("FileStorage:");
            foreach (var file in listFiles)
            {
                Debug.WriteLine($" {file}");
            }


            #region LoRa
            Configuration.SetPinFunction(41, DeviceFunction.COM2_TX);   //ESP32 S3 Atom 1
            Configuration.SetPinFunction(42, DeviceFunction.COM2_RX);   //ESP32 S3 Atom 2
            lora = E22.Create("COM2", loraAddress: loraAddress);
            if (lora != null)
            {
                lora.OnPacketReceived += (sender, e) =>
                {
                    Debug.WriteLine($"nFmem_LoRaRcv={Memory.Run(true)}");
                    try
                    {
                        var rcvData = JsonConvert.DeserializeObject(BitConverter.ToString(e.Data).FromHexString(), typeof(Rvo)) as Rvo;
                        var o = new Lamp();
                        o.LP = rcvData.LP.ToString().StringToBool();
                        o.P = rcvData.P.ToString().StringToBool();
                        o.PC = rcvData.PC == '1' ? true : false;
                        o.LU = DateTime.UtcNow.ToString("HH-mm-ss-dd-MM-yyyy");
                        o.S = rcvData.S;
                        o.SD = rcvData.SD;
                        o.SD.location = (ConvertExtencion.GetEnum(o.SD.R));
                        o.SD.lora = e.AddressID;
                        Debug.WriteLine(JsonConvert.SerializeObject(o));
                        storage.WriteData(o);
                    }
                    catch (Exception ex)
                    {
                        
                        Debug.WriteLine(ex.ToString());
                    }
                };
            }
            #endregion


            Timer RefreshFile = new Timer(WebStorage.RefreshRvo, null, 50000, 50000);


            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}
