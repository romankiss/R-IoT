using System;
using System.Diagnostics;
using System.Numerics;
using Iot.Device.Adxl343Lib;
using Iot.Device.Ssd13xx;


namespace AccelDisplay
{
    internal class Adxl343Display
    {

        public int _width;
        public int _height;
        public Ssd1306 _display;
        public Adxl343 _accel;

        public Adxl343Display(int width, int height, Ssd1306 display, Adxl343 accel){
           
            _width = width;
            _height = height;
            _display = display;
            _accel = accel;

        }

        public void WriteVector(int x, int y, Vector3 v,string text)
        {
            ///Changes the X and Y of the text based on the Vector3 variable.
            
                
                bool isLandscape = _display.Orientation == DisplayOrientation.Landscape180;

                double vectorToPixelY = (((v.X + 270.0) / 540.0) * (isLandscape ? _height : _width));
                double vectorToPixelX = ((((v.Y + 270.0) / 540.0) * (isLandscape ? _width : _height)));
                int yFromVector = (int)Math.Round(vectorToPixelY / 8);
                int xFromVector = (int)Math.Round(vectorToPixelX / 8);

                int xOffset = x / 8;
                int yOffset = y / 8;

                //Display orientation change based on Z
                if (v.Z > 0)
                {
                    _display.Orientation = DisplayOrientation.Portrait180;
                }
                else
                {
                    _display.Orientation = DisplayOrientation.Landscape180;
                }

                //Set position of text
                int xFinal = isLandscape ? xFromVector - xOffset : xFromVector - xOffset;
                int yFinal = isLandscape ? yFromVector - yOffset : yFromVector - yOffset;

                _display.Write(xFinal < 0 ? 0 : xFinal, yFinal, stringTruncating(text, xFinal, yFinal, _display));
                
        }

        private static string stringTruncating(string text, int x, int y, Ssd1306 display)
        {

            string trunctedText = null;
            int landPortMax = display.Orientation == DisplayOrientation.Landscape180 ? 16 : 8;
            int landPortYMax = display.Orientation == DisplayOrientation.Landscape180 ? 8 : 16;

            //Check if text is in the Y axis of the screen
            if (y >= 0 && y <= landPortYMax)
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
                    int newLength = landPortMax - x;

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
