using System;
using System.Diagnostics;
using System.Threading;
using System.Device.I2c;
using nanoFramework.Hardware.Esp32;
using NFAppAtomLite_Testing;//this library needs to be added
//using HT16K33;
using Iot.Device.Button;


namespace NFAppAtomLite_Testing
{
    public class Program
    {
        public static int servo_angle = 25;
        public static M5AtomicMotion motionbase;
       // public static SimpleHT16K33 ht16k33 = null;
        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");
            try
            {
                // Configure the I2C GPIOs used for the bus------------------ we need to asssign the pinst to be used for the second i2c bus by i2c2_XXXX
                Configuration.SetPinFunction(38, DeviceFunction.I2C2_DATA);//38     onboard:2  you need to check the pinout for esp32 atom s3 lite pinout - 38 is for it,,, default atom lite has diff. pinout
                Configuration.SetPinFunction(39, DeviceFunction.I2C2_CLOCK);//39    onbrd:1
            }
            catch (Exception ex)
            {
                Console.WriteLine("Bruh, exeptioon: " + ex.ToString());
            }



            //Create the button object
            GpioButton button = new GpioButton(buttonPin: 41, debounceTime: TimeSpan.FromMilliseconds(200));


            // Initialize I2C device on the default address of 0x38(Motion Base) , display ht16k33(0x70)
            I2cDevice i2c = new I2cDevice(new I2cConnectionSettings(2, M5AtomicMotion.DefaultI2cAddress));
            motionbase = M5AtomicMotion.Create(i2c);
           
            //ht16k33 = SimpleHT16K33.Create(1, BackpackWiring.Default);



            //set the angle of the servo on the 4th (0,1,2,3) channel/port to 25degrees
           // motionbase.SetServoPulse(0, 1500);
            Debug.WriteLine($"angle: {servo_angle}");

            //bind the handler with the event watcher of button
            button.Press += (sender, e) => handle_button();
            
            
            #region Encoder
            Configuration.SetPinFunction(6, DeviceFunction.I2C1_DATA);     //AtomicMotionBase SD-G19, G33
            Configuration.SetPinFunction(5, DeviceFunction.I2C1_CLOCK);    //AtomicMotionBase SC-G22, G23
            Encoder sensorEncoder = Encoder.Create(busId: 1, pollingTimeInMilliseconds: 50);
            if (sensorEncoder != null)
            {
                sensorEncoder.OnChangeValue += (sender, e) =>
                {
                    Encoder encoder = sender as Encoder;
                    if (e.OldValue != e.NewValue || e.ButtonStatus)
                    {
                        Debug.WriteLine($"sensorEncoder: Val={e.NewValue}, button={e.ButtonStatus}");
                        if (e.NewValue < M5AtomicMotion.ServoAngleBack)
                            encoder.SetLEDColor(0, (byte)(e.NewValue), 0, 0);
                        else if (e.NewValue > M5AtomicMotion.ServoAngleAhead)
                            encoder.SetLEDColor(0, 0, 0, (byte)(e.NewValue));
                        else
                        {
                            encoder.SetLEDColor(0, 0, (byte)(e.NewValue), 0);
                            if (motionbase != null && e.ButtonStatus)
                            {
                                Debug.WriteLine($"ServoAngle motion = {e.NewValue}");
                                motionbase.SetServoAngle(0, (byte)(e.NewValue));
                                /*if (sensorToF != null)
                                {
                                    Debug.WriteLine($"Distance: {sensorToF.Distance} mm");
                                }*/
                            }
                        }
                    }
                };
            }
            #endregion





            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }

        public static void handle_button()
        {
           /* //get the current angle of the servo
            servo_angle = servo.GetServoAngle(3);
            if (servo_angle >= 85)
            {
                //if the clamp is already closed, open it again
                servo_angle = 25;
            }
            else
            {
                //if the clamp is not fully closed yet close it a bit
                servo_angle += 5;
            }
            servo.SetServoAngle(3, (byte)servo_angle);
            Debug.WriteLine($"angle: {servo_angle}");

            */

            /*if (ht16k33 != null)
            {
                ht16k33.Init();
                ht16k33.SetBrightness(10);
                ht16k33.SetBlinkRate();
                ht16k33.Clear();

                ht16k33.ShowMessage("SPSE Piestany", true, 100);
                //ht16k33.ShowMatrix(0x183c7effffff6600, false, 100);
                //ht16k33.ShowAndCirculateMessageAsync("", 250);
                //ht16k33.ShowArray(buffer, bScrollLastCharacters: false);

            }
            else
            {
                Debug.WriteLine("================");
            }**/
        }
    }
}
