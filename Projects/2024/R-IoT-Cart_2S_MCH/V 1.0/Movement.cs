// written by Miroslav Chudý, October 17th, 2024,
// version: 1.0.0.0 October 17th,2024


using System.Threading;
using nanoFramework.Device.Bluetooth.Advertisement;
using NFAppAtomLite_Testing;
using System.Diagnostics;

namespace S3Movement
{




    public class Car
    {
        private M5AtomicMotion _motor;
        private static Stopwatch _clock;


        public Car(M5AtomicMotion motor)
        {
            _motor = motor;
            _clock = Stopwatch.StartNew();
        }

        public void testDrive()
        {
            _clock.Reset();
            _clock.Start();
            _motor.SetServoAngle(2, 135);
            while (_clock.ElapsedMilliseconds != 1000)
            {
                Debug.WriteLine(_clock.ElapsedMilliseconds.ToString());
            }
            _motor.SetServoAngle(2, 90);
        }

        public void forward()
        {
            _motor.SetServoAngle(0, 0);
            _motor.SetServoAngle(2, 180);
            
            return;
        }

        public void stop()
        {
            _motor.SetServoAngle(0, 90);
            _motor.SetServoAngle(2, 90);

            return;
        }

        public void turnSquareRight()
        {
            _clock.Reset();
            _clock.Start();
            _motor.SetServoAngle(2, 135);

            while (_clock.ElapsedMilliseconds != 1620)
            {
              //  Debug.WriteLine(_clock.ElapsedMilliseconds.ToString());
            }

            _motor.SetServoAngle(2, 90);
        }

        public void turnSquareLeft()
        {
            _clock.Reset();
            _clock.Start();
            _motor.SetServoAngle(0, 45);

            while (_clock.ElapsedMilliseconds != 1620)
            {
              //  Debug.WriteLine(_clock.ElapsedMilliseconds.ToString());
            }

            _motor.SetServoAngle(0, 90);
        }

        public void turnAround()
        {
            _clock.Reset();
            _clock.Start();
            _motor.SetServoAngle(0, 45);

            while (_clock.ElapsedMilliseconds != 3240)
            {
             //   Debug.WriteLine(_clock.ElapsedMilliseconds.ToString());
            }


            _motor.SetServoAngle(0, 90);


        }


        public void turnRight(int angle)
        {
            int turnTime = (angle * 1620) / 90;
            _clock.Reset();
            _clock.Start();
            _motor.SetServoAngle(2, 135);
           
            while (_clock.ElapsedMilliseconds != turnTime) 
            {
               // Debug.WriteLine(_clock.ElapsedMilliseconds.ToString());
            }

            _motor.SetServoAngle(2, 90);
        }

        public void turnLeft(int angle)
        {
            int turnTime = (angle * 1520) / 90;
            _motor.SetServoAngle(0, 45);
            Thread.Sleep(turnTime);
            _motor.SetServoAngle(0,90);
        }

        public void turnRightContinuous()
        {
            _motor.SetServoAngle(2, 135);
        }

        public void moveToDistance(int distance)
        {
            //86 average distance per second
            int time = (distance / 86) * 1000;
            this.forward();
            Thread.Sleep(time);
            this.stop();
        }


    }
}