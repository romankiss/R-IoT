using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using CanSat;

namespace CanSatU1
{
    internal class ServoManager
    {
    
        private static readonly object _lock = new object();
        private bool _disposed = false;
        private M5AtomicMotion motion = null;
        public ServoManager (M5AtomicMotion motionBase)
        {
            motion = motionBase;
        }
        public void SetServoAngle(byte angle, byte servoChannel = 1)
        {
            lock (_lock)
            {
                motion.SetServoAngle(servoChannel, angle);
            }
        }
        public void TestServo(byte servoChannel = 1, int Times = 1)
        {
            lock (_lock)
            {
                for (int i = 0; i < Times; i++)
                {
                    SetServoAngle(M5AtomicMotion.ServoAngleBack, servoChannel);
                    Thread.Sleep(1000);
                    SetServoAngle(M5AtomicMotion.ServoAngleStop, servoChannel);
                    Thread.Sleep(1000);
                    SetServoAngle(M5AtomicMotion.ServoAngleAhead, servoChannel);
                    Thread.Sleep(1000);
                }
            }
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
                    // Free managed resources
                    
                }
                // Free unmanaged resources
                _disposed = true;
            }
        }
    }
}
