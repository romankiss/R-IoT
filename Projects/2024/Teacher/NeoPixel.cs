
// code snippet for NeoPixel (LED)
// nuget assembly: nanoFramework.Iot.Device.Ws28xx.Esp32

// based on https://github.com/nanoframework/nanoFramework.IoT.Device/tree/develop/devices/Ws28xx.Esp32


using System;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Threading;
using Iot.Device.Ws28xx.Esp32;
using nanoFramework.Hardware.Esp32;

// Configure the count of pixels
const int Count = 1; 
// Adjust the pin number, where is the NeoPixel connected. 
const int Pin = 10;


//
// usage of Ws2808 driver    
//
Ws28xx neo = new Ws2808(Pin, Count);
//neo.Image.SetPixel(0, 0, Color.Blue);
neo.Image.SetPixel(0, 0, 10, 0, 0);
neo.Update();
Thread.Sleep(1000);
neo.Image.SetPixel(0, 0, Color.Black);
neo.Update();


// more funny with NeoPixel
//Rainbow(neo, Count);




// function 
static void Rainbow(Ws28xx neo, int count, int iterations = 10)
{
     BitmapImage img = neo.Image;
     for (var i = 0; i < 255 * iterations; i++)
     {
          for (var j = 0; j < count; j++)
          {
              img.SetPixel(j, 0, Wheel((i + j) & 255));
          }
          neo.Update();
      }
  }




