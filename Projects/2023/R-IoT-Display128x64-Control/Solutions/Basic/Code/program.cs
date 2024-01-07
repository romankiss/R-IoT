using System;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Ssd13xx;
using Iot.Device.Adxl343Lib;
using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using System.Numerics;

namespace AccelDisplay
{
    public class Program
    {

        public static Ssd1306 displayConfig()
        {
            Configuration.SetPinFunction(25, DeviceFunction.I2C2_DATA);
            Configuration.SetPinFunction(21, DeviceFunction.I2C2_CLOCK);
    
            var i2cDisplay = I2cDevice.Create(new I2cConnectionSettings(2, 0x3C));
            var display = new Iot.Device.Ssd13xx.Ssd1306(i2cDisplay);
            display.Font = new Sinclair8x8();

            return display;
        }
        
        public static Adxl343 accelConfig() 
        {
            Configuration.SetPinFunction(22, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(19, DeviceFunction.I2C1_CLOCK);

            var i2cAccel = I2cDevice.Create(new I2cConnectionSettings(1, 0x53));
            Adxl343 sensor = new Adxl343(i2cAccel, GravityRange.Range16);

            return sensor;
        }


        public static void Main()
        {
            //Display
            Ssd1306 display = displayConfig();
            display.Orientation = DisplayOrientation.Landscape180;

            //Accel
            Adxl343 sensor = accelConfig();
            Vector3 v = new Vector3();

            //Text, viac sa neda kvoli pomalemu procesingu displejovania, pravdepodobne mozne neskor opravit
            string li1 = "Testovanie displeja";
            string li2 = "viac textu sa neda";
           
            //Text truncating, vector measuring and orientation change loop
            while (true)
            {
               
                if (sensor.TryGetAcceleration(ref v)) {

                    bool isLandscape = display.Orientation == DisplayOrientation.Landscape180;

                    //Vector to screen pixel conversion
                    double vectorToPixelY = (((v.X + 270.0) / 540.0) * (isLandscape ? 128.0 : 256.0) );
                    double vectorToPixelX = ((((v.Y + 270.0) / 540.0) * (isLandscape ? 256.0 : 128.0) ));
                    int y = (int)Math.Round(vectorToPixelY/8);
                    int x = (int)Math.Round(vectorToPixelX/8);


                    //Display orientation change based on Z
                    if (v.Z > 0)
                    {
                        display.Orientation = DisplayOrientation.Portrait180;
                    }
                    else
                    {
                        display.Orientation = DisplayOrientation.Landscape180;
                    }


                    //Set position of text
                    //256 = 32, 128 = 16, 64 = 8
                    int x1 = isLandscape ? x - 10 : x - 16;
                    int y1 = isLandscape ? y - 6 : y - 16;

                    int x2 = isLandscape ? x - 10 : x - 16;
                    int y2 = isLandscape ? y - 5 : y - 15;


                    //Display text
                    display.ClearScreen();
                    display.Write(x1 < 0 ? 0 : x1, y1, stringTruncating(li1, x1, y1, display));
                    display.Write(x2 < 0 ? 0 : x2, y2, stringTruncating(li2, x2, y2, display));
                    display.Display();
                    
                    Thread.Sleep(20);
                }
                
            }

        }

        private static string stringTruncating(string text, int x, int y, Ssd1306 display)
        {

            
            string trunctedText = null;
            int landPortMax = display.Orientation == DisplayOrientation.Landscape180 ? 16 : 8;

            //Check if text is in the Y axis of the screen
            if (y >= 0 && y <= 8) 
            {

                //Check if the text is being truncated from the left or right
                if (x <= 0) 
                {
                    int visibleLength = Math.Min(text.Length + x, landPortMax);
                    if (visibleLength > 0)
                    {
                        trunctedText = text.Substring(Math.Max(0, -x), visibleLength);
                        Debug.WriteLine(trunctedText);
                        Debug.WriteLine(Math.Abs(x).ToString());
                        return trunctedText; 
                    }
                    else
                    {
                        Debug.WriteLine("No text to display");
                        return "";
                    }

                }
                else if (text.Length > landPortMax - x)
                {
                    int newLength = landPortMax  - x;

                    if (newLength > 0)
                    {
                        trunctedText = text.Substring(0, Math.Min(landPortMax, newLength));
                        Debug.WriteLine(trunctedText);
                        return trunctedText;
                    }
                    else
                    {
                        Debug.WriteLine("No text to display");
                        return "";
                    }

                }
                else
                {
                    Debug.WriteLine("No truncating needed");
                    return text;
                }

            }
            else
            {
                Debug.WriteLine("Not on screen height");
                return "";
            }

        }


    }
}
