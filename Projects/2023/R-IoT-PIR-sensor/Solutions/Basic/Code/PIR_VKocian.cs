using System;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Hcsr501;
using System.Device.Gpio;


namespace utrazvuk
{

    // https://docs.nanoframework.net/devicesdetails/Mfrc522/README.html
    // https://github.com/nanoframework/nanoFramework.IoT.Device/blob/develop/devices/Mfrc522/samples/Program.cs

    public class Program
    {
        static Hcsr501 sensorPIR = null;
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            #region PIR
            sensorPIR = new Hcsr501(25);    // G26 on the Hat, Atom Grove G32 
            sensorPIR.Hcsr501ValueChanged += (s, e) =>
            {
                if (e.PinValue == PinValue.High)
                {

                    Debug.WriteLine("pohyb");
                }
            };
            #endregion

            Thread.Sleep(Timeout.Infinite);
        }


    }



}

