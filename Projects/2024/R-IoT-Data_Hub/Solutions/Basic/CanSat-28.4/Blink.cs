using System;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Ws28xx.Esp32;

namespace CanSat-28.4
{
    internal class Blink : IDisposable
    {
        private static Sk6812 _neo;
        private static readonly object _lock = new object();
        private bool _disposed = false;

        public Blink(int gpioPin = 35, int pixelCount = 1)
        {
            if (_neo == null)
            {
                _neo = new Sk6812(gpioPin, pixelCount);
            }
        }

        public void BlinkLed(byte r, byte g, byte b, int periodInMs = 100,
                           double dutyCycle = 0.5, int times = 1)
        {
            ValidateParameters(dutyCycle, periodInMs);

            lock (_lock)
            {
                try
                {
                    for (int i = 0; i < times; i++)
                    {
                        SetLedColor(r, g, b);
                        Thread.Sleep((int)(periodInMs * dutyCycle));

                        TurnOffLed();

                        if (i < times - 1)
                        {
                            Thread.Sleep((int)(periodInMs * (1 - dutyCycle)));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"LED blink failed: {ex.Message}");
                    throw;
                }
            }
        }

        public Thread BlinkLedAsync(byte r, byte g, byte b, int periodInMs = 100,
                                  double dutyCycle = 0.5, int times = 1)
        {
            var blinkThread = new Thread(() =>
            {
                try
                {
                    BlinkLed(r, g, b, periodInMs, dutyCycle, times);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Async LED blink failed: {ex.Message}");
                }
            })
            {
                Priority = ThreadPriority.BelowNormal,
                //IsBackground = true
            };

            blinkThread.Start();
            return blinkThread;
        }

        private void SetLedColor(byte r, byte g, byte b)
        {
            if (_neo?.Image == null)
                throw new InvalidOperationException("LED controller not initialized");

            _neo.Image.SetPixel(0, 0, r, g, b);
            _neo.Update();
        }

        private void TurnOffLed()
        {
            if (_neo?.Image == null)
                throw new InvalidOperationException("LED controller not initialized");

            _neo.Image.Clear();
            _neo.Update();
        }

        private void ValidateParameters(double dutyCycle, int periodInMs)
        {
            if (dutyCycle <= 0 || dutyCycle > 1)
                throw new ArgumentOutOfRangeException(
                    nameof(dutyCycle), "Duty cycle must be between 0 and 1");

            if (periodInMs <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(periodInMs), "Period must be greater than 0");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Turn off LED before disposal
                    try
                    {
                        TurnOffLed();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error turning off LED during disposal: {ex.Message}");
                    }

                    // No Dispose() call needed for Sk6812
                }
                _disposed = true;
            }
        }

        ~Blink()
        {
            Dispose(false);
        }
    }
}