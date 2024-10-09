using System;
using System.Diagnostics;
using System.Threading;
//creates a "driver" that runs on a second thread besides the main, generates new values every second to be send to the main thread.
namespace Zko
{
    public class Program
    {

        

        public delegate void OnXEvent(object source, OnXEventArgs e);

        // interesting event
        public class OnXEventArgs : EventArgs
        {
            public int X { get; set; }
            public int Y { get; set; }
            // ... add more based on the needs
        }

        // my driver
        public class MyDriver
        {
            public event OnXEvent OnChangeValue = null;
            Random randomGenerator = new Random();
            public bool IsThereChange = false;
            public int oldX, oldY = 333;

            //Constructor
            public MyDriver()
            {
                //...
                var task = new Thread(() =>
                {
                    while (true)
                    {
                        // polling my event, compare old and new values, store new valuse, set flag IsThereChange

                        var newX = randomGenerator.Next(100);
                        var newY = randomGenerator.Next(100);

                        if (newX != oldX || newY != oldY)
                        {
                            IsThereChange = true;
                        }
                        else
                        {
                            IsThereChange = false;
                        }
                        oldX = newX; oldY = newY;   

                        if (OnChangeValue != null && IsThereChange)
                        {
                            var args = new OnXEventArgs() { X = newX, Y = newY };
                            OnChangeValue.Invoke(this, args);
                        }
                        Thread.Sleep(1000);

                    }
                }){ Priority = ThreadPriority.BelowNormal };
                task.Start();
            }

        }
    public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            // Mastering Delegates and Events In C# .NetFramework
            // https://www.c-sharpcorner.com/UploadFile/84c85b/delegates-and-events-C-Sharp-net/
            //
            // Events

            // usage of events in the application
            MyDriver mydriver = new MyDriver();
            if (mydriver != null)
            {
                mydriver.OnChangeValue += (sender, e) =>
                {
                    // to do
                    Debug.WriteLine($"Event happened and send by drver, values: {e.X}, {e.Y}.");
                    Blink.Blinks(255, 255, 0, 1000, 0.5, 1);
                };



            }


            Thread.Sleep(Timeout.Infinite);

            // Browse our samples repository: https://github.com/nanoframework/samples
            // Check our documentation online: https://docs.nanoframework.net/
            // Join our lively Discord community: https://discord.gg/gCyBu8T
        }
    }
}
